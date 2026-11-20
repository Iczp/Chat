using IczpNet.Chat.SessionUnits;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using IczpNet.Chat.MessageSections.Messages;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.CallCenters;

public interface ICallCenterManager : IDomainService
{
    /// <summary>Count assigned conversations whose last activity is within the configured window.</summary>
    Task<Dictionary<long, int>> GetActiveCountsAsync(long shopKeeperId, IReadOnlyCollection<long> agentIds);
    Task ProcessMessageAsync(Message message);
    Task<CallCenterAssignment> GetAssignmentAsync(Guid sessionId);
    Task<int> DrainQueueAsync(long shopKeeperId, int maxCount = 50);
    Task CancelQueueAsync(Guid sessionId, long customerId);

    /// <summary>
    /// 转接
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="waiterId"></param>
    /// <param name="isNotice"></param>
    /// <returns></returns>
    Task<SessionUnit> TransferToAsync(Guid sessionUnitId, long waiterId, bool isNotice = true);
}
