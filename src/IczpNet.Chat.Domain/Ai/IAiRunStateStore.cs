using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Ai;

public interface IAiRunStateStore : ITransientDependency
{
    Task UpsertAsync(AiRunState state, CancellationToken cancellationToken = default);
    Task<AiRunState> GetAsync(Guid requesterSessionUnitId, CancellationToken cancellationToken = default);
    Task<List<AiRunState>> GetRecentAsync(Guid requesterSessionUnitId, int maxResultCount = 20, CancellationToken cancellationToken = default);
}
