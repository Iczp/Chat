using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.RedisServices;
using Volo.Abp.Caching;
using IczpNet.Chat.AiRuns;

namespace IczpNet.Chat.Ai;

/// <summary>
/// Redis-backed IM AI run state. Hash holds each run payload and ZSet holds the
/// newest-first index. Upsert is one Lua transaction, so concurrent stream
/// events from separate workers cannot overwrite one another.
/// </summary>
public class RedisAiRunStateStore(
    IDistributedCache<List<AiRunState>, string> legacyCache) : RedisService, IAiRunStateStore
{
    private readonly IDistributedCache<List<AiRunState>, string> _legacyCache = legacyCache;
    private const int MaximumRuns = 50;
    private static readonly TimeSpan RunningExpiry = TimeSpan.FromHours(24);
    private static readonly TimeSpan TerminalExpiry = TimeSpan.FromHours(1);

    private const string UpsertScript = @"
local existingScore = redis.call('ZSCORE', KEYS[2], ARGV[1])
if existingScore and tonumber(existingScore) > tonumber(ARGV[3]) then
    return 0
end
redis.call('HSET', KEYS[1], ARGV[1], ARGV[2])
redis.call('ZADD', KEYS[2], ARGV[3], ARGV[1])
local count = redis.call('ZCARD', KEYS[2])
if count > tonumber(ARGV[4]) then
    local removed = redis.call('ZRANGE', KEYS[2], 0, count - tonumber(ARGV[4]) - 1)
    if #removed > 0 then
        redis.call('HDEL', KEYS[1], unpack(removed))
        redis.call('ZREM', KEYS[2], unpack(removed))
    end
end
redis.call('PEXPIRE', KEYS[1], ARGV[5])
redis.call('PEXPIRE', KEYS[2], ARGV[5])
return 1";

    private const string ApplyEventScript = @"
local current = redis.call('HGET', KEYS[1], ARGV[1])
local state
if current then
    state = cjson.decode(current)
    local curSeq = -1
    if state and type(state.sequence) == 'number' then
        curSeq = state.sequence
    elseif state and type(state.sequence) == 'string' then
        curSeq = tonumber(state.sequence) or -1
    end
    if curSeq >= tonumber(ARGV[11]) then
        return 0
    end
else
    state = cjson.decode(ARGV[2])
end
if not state or type(state) ~= 'table' or state.runId ~= ARGV[1] then state = {} end
state.runId = ARGV[1]
state.sessionId = ARGV[3]
state.requesterSessionUnitId = ARGV[4]
state.sourceMessageId = tonumber(ARGV[5])
state.requestedAt = ARGV[6]
state.startedAt = ARGV[7]
state.updatedAt = ARGV[8]
state.queueMilliseconds = tonumber(ARGV[9])
state.elapsedMilliseconds = tonumber(ARGV[10])
state.sequence = tonumber(ARGV[11])
state.status = ARGV[12]

local preview = ''
if type(state.previewText) == 'string' then
    preview = state.previewText
end
if ARGV[14] ~= '' then
    preview = preview .. ARGV[14]
    if string.len(preview) > 2048 then preview = string.sub(preview, -2048) end
end
state.previewText = preview

if ARGV[15] ~= '' then state.finalMessageId = tonumber(ARGV[15]) elseif type(state.finalMessageId) ~= 'number' then state.finalMessageId = nil end
if ARGV[16] ~= '' then state.error = ARGV[16] elseif type(state.error) ~= 'string' then state.error = nil end
if type(state.timeline) ~= 'table' then state.timeline = {} end
local detail = ARGV[16] ~= '' and ARGV[16] or (ARGV[14] ~= '' and ('delta:' .. string.len(ARGV[14]) .. ' chars') or (ARGV[15] ~= '' and ('finalMessageId:' .. ARGV[15]) or ''))
table.insert(state.timeline, {
    occurredAt = ARGV[8], eventType = ARGV[13], status = ARGV[12],
    sequence = tonumber(ARGV[11]), elapsedMilliseconds = tonumber(ARGV[10]), detail = detail
})
while #state.timeline > 200 do table.remove(state.timeline, 1) end
redis.call('HSET', KEYS[1], ARGV[1], cjson.encode(state))
redis.call('ZADD', KEYS[2], ARGV[17], ARGV[1])
local count = redis.call('ZCARD', KEYS[2])
if count > tonumber(ARGV[18]) then
    local removed = redis.call('ZRANGE', KEYS[2], 0, count - tonumber(ARGV[18]) - 1)
    if #removed > 0 then
        redis.call('HDEL', KEYS[1], unpack(removed))
        redis.call('ZREM', KEYS[2], unpack(removed))
    end
end
redis.call('PEXPIRE', KEYS[1], ARGV[19])
redis.call('PEXPIRE', KEYS[2], ARGV[19])
return 1";

    private static readonly JsonSerializerOptions StorageJsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<AiRunState> GetAsync(Guid requesterSessionUnitId, CancellationToken cancellationToken = default)
        => (await GetRecentAsync(requesterSessionUnitId, MaximumRuns, cancellationToken))
            .FirstOrDefault(x => x.Status is "queued" or "running" or "streaming");

    public async Task<List<AiRunState>> GetActiveAsync(IEnumerable<Guid> requesterSessionUnitIds, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sessionUnitIds = requesterSessionUnitIds?.Where(x => x != Guid.Empty)
            .Distinct().Take(MaximumRuns).ToList() ?? [];
        if (sessionUnitIds.Count == 0) return [];

        // One Redis pipeline obtains the newest run IDs for every visible
        // session. A second pipeline reads their payloads. This avoids the
        // N+1 HTTP pattern while keeping Redis keys scoped per conversation.
        var indexBatch = Database.CreateBatch();
        var indexTasks = sessionUnitIds.ToDictionary(
            sessionUnitId => sessionUnitId,
            sessionUnitId => indexBatch.SortedSetRangeByRankAsync(
                IndexKey(sessionUnitId), 0, MaximumRuns - 1, Order.Descending));
        indexBatch.Execute();
        await Task.WhenAll(indexTasks.Values);
        cancellationToken.ThrowIfCancellationRequested();

        var runIdsBySession = indexTasks
            .Select(x => new { x.Key, RunIds = x.Value.Result })
            .Where(x => x.RunIds.Length > 0)
            .ToList();
        if (runIdsBySession.Count == 0) return [];

        var runsBatch = Database.CreateBatch();
        var runTasks = runIdsBySession.ToDictionary(
            item => item.Key,
            item => runsBatch.HashGetAsync(RunsKey(item.Key), item.RunIds));
        runsBatch.Execute();
        await Task.WhenAll(runTasks.Values);

        return runTasks.Values.SelectMany(task => task.Result)
            .Where(value => value.HasValue)
            .Select(value => JsonSerializer.Deserialize<AiRunState>(value.ToString(), StorageJsonOptions))
            .Where(state => state?.Status is "queued" or "running" or "streaming")
            .ToList();
    }

    public async Task<AiRunState> GetByRunIdAsync(Guid requesterSessionUnitId, string runId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(runId)) return null;
        cancellationToken.ThrowIfCancellationRequested();
        var value = await Database.HashGetAsync(RunsKey(requesterSessionUnitId), runId);
        if (value.HasValue)
        {
            return JsonSerializer.Deserialize<AiRunState>(value.ToString(), StorageJsonOptions);
        }

        // A currently running request written before this release can still
        // be continued safely until the old cache expires.
        return (await _legacyCache.GetAsync(LegacyKey(requesterSessionUnitId), token: cancellationToken) ?? [])
            .FirstOrDefault(x => string.Equals(x.RunId, runId, StringComparison.Ordinal));
    }

    public async Task<List<AiRunState>> GetRecentAsync(Guid requesterSessionUnitId, int maxResultCount = 20, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var count = Math.Clamp(maxResultCount, 1, MaximumRuns);
        var entries = await Database.SortedSetRangeByRankAsync(IndexKey(requesterSessionUnitId), 0, count - 1, Order.Descending);
        if (entries.Length == 0)
        {
            // One-release compatibility with the prior list-cache key. New
            // writes always use the atomic Hash/ZSet representation.
            return (await _legacyCache.GetAsync(LegacyKey(requesterSessionUnitId), token: cancellationToken) ?? [])
                .OrderByDescending(x => x.UpdatedAt).Take(count).ToList();
        }

        var values = await Database.HashGetAsync(RunsKey(requesterSessionUnitId), entries);
        return values.Where(x => x.HasValue)
            .Select(x => JsonSerializer.Deserialize<AiRunState>(x.ToString(), StorageJsonOptions))
            .Where(x => x != null)
            .ToList();
    }

    public async Task UpsertAsync(AiRunState state, CancellationToken cancellationToken = default)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (string.IsNullOrWhiteSpace(state.RunId)) throw new ArgumentException("RunId is required.", nameof(state));
        cancellationToken.ThrowIfCancellationRequested();

        var expiry = state.Status is "completed" or "failed" or "cancelled" ? TerminalExpiry : RunningExpiry;
        // Unix milliseconds plus a bounded sequence preserves stream ordering
        // while staying inside Redis double's exact integer range.
        var updatedAt = state.UpdatedAt == default ? DateTime.UtcNow : state.UpdatedAt.ToUniversalTime();
        var unixMilliseconds = new DateTimeOffset(updatedAt).ToUnixTimeMilliseconds();
        var score = unixMilliseconds * 1000d + Math.Clamp(state.Sequence, 0, 999);
        await Database.ScriptEvaluateAsync(UpsertScript,
            [RunsKey(state.RequesterSessionUnitId), IndexKey(state.RequesterSessionUnitId)],
            [state.RunId, JsonSerializer.Serialize(state, StorageJsonOptions), score, MaximumRuns, (long)expiry.TotalMilliseconds]);
    }

    public async Task ApplyEventAsync(AiRunStateEvent runEvent, AiRunState legacySeed = null, CancellationToken cancellationToken = default)
    {
        if (runEvent == null) throw new ArgumentNullException(nameof(runEvent));
        if (string.IsNullOrWhiteSpace(runEvent.RunId)) throw new ArgumentException("RunId is required.", nameof(runEvent));
        cancellationToken.ThrowIfCancellationRequested();

        var occurredAt = runEvent.OccurredAt == default ? DateTime.UtcNow : runEvent.OccurredAt.ToUniversalTime();
        var unixMilliseconds = new DateTimeOffset(occurredAt).ToUnixTimeMilliseconds();
        var score = unixMilliseconds * 1000d + Math.Clamp(runEvent.Sequence, 0, 999);
        var expiry = runEvent.Status is "completed" or "failed" or "cancelled" ? TerminalExpiry : RunningExpiry;
        var seed = legacySeed ?? new AiRunState { RunId = runEvent.RunId, Timeline = [] };
        await Database.ScriptEvaluateAsync(ApplyEventScript,
            [RunsKey(runEvent.RequesterSessionUnitId), IndexKey(runEvent.RequesterSessionUnitId)],
            [runEvent.RunId, JsonSerializer.Serialize(seed, StorageJsonOptions), runEvent.SessionId.ToString(), runEvent.RequesterSessionUnitId.ToString(),
                runEvent.SourceMessageId, runEvent.RequestedAt.ToUniversalTime().ToString("O"), runEvent.StartedAt.ToUniversalTime().ToString("O"), occurredAt.ToString("O"),
                runEvent.QueueMilliseconds, runEvent.ElapsedMilliseconds, runEvent.Sequence, runEvent.Status ?? string.Empty, runEvent.EventType ?? string.Empty,
                runEvent.Delta ?? string.Empty, runEvent.FinalMessageId?.ToString() ?? string.Empty, runEvent.Error ?? string.Empty,
                score, MaximumRuns, (long)expiry.TotalMilliseconds]);
    }

    private RedisKey RunsKey(Guid requesterSessionUnitId) => $"{Options.Value.KeyPrefix}AiRuns:SessionUnit:{requesterSessionUnitId:N}:Runs";
    private RedisKey IndexKey(Guid requesterSessionUnitId) => $"{Options.Value.KeyPrefix}AiRuns:SessionUnit:{requesterSessionUnitId:N}:Index";
    private static string LegacyKey(Guid requesterSessionUnitId) => $"AiRuns:SessionUnit:{requesterSessionUnitId:N}";
}
