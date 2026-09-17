using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.AiRuns;

public interface IAiRunStateStore : ITransientDependency
{
    Task UpsertAsync(AiRunState state, CancellationToken cancellationToken = default);
    /// <summary>
    /// Atomically folds one stream lifecycle event into its Run snapshot.
    /// Implementations must merge preview/timeline server-side, never through
    /// a caller-side read-modify-write cycle.
    /// </summary>
    Task ApplyEventAsync(AiRunStateEvent runEvent, AiRunState legacySeed = null, CancellationToken cancellationToken = default);
    /// <summary>Gets one exact run; never substitutes another active run in the same session.</summary>
    Task<AiRunState> GetByRunIdAsync(Guid requesterSessionUnitId, string runId, CancellationToken cancellationToken = default);
    Task<AiRunState> GetAsync(Guid requesterSessionUnitId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets active runs for the small, already-authorized viewport set. The
    /// Redis implementation pipelines this instead of issuing one HTTP/API
    /// query per conversation.
    /// </summary>
    Task<List<AiRunState>> GetActiveAsync(IEnumerable<Guid> requesterSessionUnitIds, CancellationToken cancellationToken = default);
    Task<List<AiRunState>> GetRecentAsync(Guid requesterSessionUnitId, int maxResultCount = 20, CancellationToken cancellationToken = default);
}
