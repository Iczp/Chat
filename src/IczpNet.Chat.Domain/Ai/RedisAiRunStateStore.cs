using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Ai;

/// <summary>Unified Redis state for any IM AI provider, keyed by requester unit.</summary>
public class RedisAiRunStateStore(IDistributedCache<List<AiRunState>, string> cache) : DomainService, IAiRunStateStore
{
    private readonly IDistributedCache<List<AiRunState>, string> _cache = cache;
    private static readonly DistributedCacheEntryOptions RunningOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) };
    private static readonly DistributedCacheEntryOptions TerminalOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };

    public async Task<AiRunState> GetAsync(Guid requesterSessionUnitId, CancellationToken cancellationToken = default)
        => (await GetRecentAsync(requesterSessionUnitId, 50, cancellationToken))
            .Find(x => x.Status is "queued" or "running" or "streaming");

    public async Task<List<AiRunState>> GetRecentAsync(Guid requesterSessionUnitId, int maxResultCount = 20, CancellationToken cancellationToken = default)
        => (await _cache.GetAsync(Key(requesterSessionUnitId), token: cancellationToken) ?? [])
            .OrderByDescending(x => x.UpdatedAt).Take(Math.Clamp(maxResultCount, 1, 50)).ToList();

    public async Task UpsertAsync(AiRunState state, CancellationToken cancellationToken = default)
    {
        var runs = await _cache.GetAsync(Key(state.RequesterSessionUnitId), token: cancellationToken) ?? [];
        var previous = runs.Find(x => x.RunId == state.RunId);
        if (previous != null) runs.Remove(previous);
        runs.Add(state);
        runs = runs.OrderByDescending(x => x.UpdatedAt).Take(50).ToList();
        await _cache.SetAsync(Key(state.RequesterSessionUnitId), runs,
            state.Status is "completed" or "failed" or "cancelled" ? TerminalOptions : RunningOptions,
            token: cancellationToken);
    }

    private static string Key(Guid requesterSessionUnitId) => $"AiRuns:SessionUnit:{requesterSessionUnitId:N}";
}
