using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionUnitCounters
{
    public interface ISessionUnitCounterRepository
    {
        Task<int> IncrementPublicBadgeAndRemindAllCountAndUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, DateTime messageCreationTime, Guid? ignoreSessionUnitId, bool isRemindAll);

        Task<int> IncrementPrivateBadgeAndUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, DateTime messageCreationTime, List<Guid> destinationSessionUnitIdList);

        Task<int> IncrementRemindMeCountAsync(Guid sessionId, DateTime messageCreationTime, List<Guid> destinationSessionUnitIdList);

        Task<int> IncrementFollowingCountAsync(Guid sessionId, DateTime messageCreationTime, List<Guid> destinationSessionUnitIdList);

    }
}
