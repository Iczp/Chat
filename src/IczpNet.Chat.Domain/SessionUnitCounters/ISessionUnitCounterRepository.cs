using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionUnitCounters;

public interface ISessionUnitCounterRepository
{
    Task<int> IncrementPublicBadgeAndRemindAllCountAndUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, DateTime messageCreationTime, Guid senderSessionUnitId, bool isRemindAll);

    Task<int> IncrementPrivateBadgeAndUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, DateTime messageCreationTime, Guid senderSessionUnitId, List<Guid> destinationSessionUnitIdList);

    Task<int> IncrementRemindMeCountAsync(Guid sessionId, DateTime messageCreationTime, List<Guid> destinationSessionUnitIdList);

    Task<int> IncrementFollowingCountAsync(Guid sessionId, DateTime messageCreationTime, List<Guid> destinationSessionUnitIdList);

}
