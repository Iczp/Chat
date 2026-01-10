using System.Threading.Tasks;
using System;
using Volo.Abp.Domain.Repositories;
using System.Collections.Generic;
using IczpNet.Chat.SessionSections.SessionUnits;

namespace IczpNet.Chat.SessionUnits;

public interface ISessionUnitRepository : IRepository<SessionUnit, Guid>
{

    //Task<int> BatchUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null);

    Task<int> UpdateLastMessageIdAsync(Guid senderSessionUnitId, long lastMessageId);

    Task<int> UpdateTicksAsync(Guid senderSessionUnitId, long? ticks);

    Task<int> IncrementPublicBadgeAndRemindAllCountAndUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, DateTime messageCreationTime, Guid senderSessionUnitId, bool isRemindAll);

    Task<int> IncrementPrivateBadgeAndUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, DateTime messageCreationTime, Guid senderSessionUnitId, List<Guid> destinationSessionUnitIdList);

    Task<int> IncrementRemindMeCountAsync(Guid sessionId, DateTime messageCreationTime, List<Guid> destinationSessionUnitIdList);

    Task<int> IncrementFollowingCountAsync(Guid sessionId, DateTime messageCreationTime, List<Guid> destinationSessionUnitIdList);

    //Task<int> BatchUpdateNameAsync(long chatObjectId, string name, string nameSpelling, string nameSpellingAbbreviation);

    Task<int> BatchUpdateAppUserIdAsync(long chatObjectId, Guid appUserId);

    /// <summary>
    /// 更新记数器
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    Task<SessionUnit> UpdateCountersync(SessionUnitCounterInfo info);

    /// <summary>
    /// 清除角标
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<long> ClearBadgeAsync(long ownerId);
}
