using System.Threading.Tasks;
using System;
using Volo.Abp.Domain.Repositories;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public interface ISessionUnitRepository : IRepository<SessionUnit, Guid>
    {

        Task<int> BatchUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null);

        Task<int> BatchUpdatePublicBadgeAsync(Guid sessionId, DateTime messageCreationTime, Guid ignoreSessionUnitId);

        Task<int> BatchUpdatePrivateBadgeAsync(Guid sessionId, DateTime messageCreationTime, Guid receiverSessionUnitId);

        Task<int> BatchUpdateRemindMeCountAsync(DateTime messageCreationTime, List<Guid> sessionUnitIdList);

        Task<int> BatchUpdateRemindAllCountAsync(Guid sessionId, DateTime messageCreationTime, Guid ignoreSessionUnitId);

        Task<Dictionary<Guid, SessionUnitStatModel>> GetStatsAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);
        
    }
}
