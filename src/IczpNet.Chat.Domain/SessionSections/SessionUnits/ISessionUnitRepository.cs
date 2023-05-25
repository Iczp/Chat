using System.Threading.Tasks;
using System;
using Volo.Abp.Domain.Repositories;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public interface ISessionUnitRepository : IRepository<SessionUnit, Guid>
    {

        //Task<int> BatchUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null);

        Task<int> BatchUpdateLastMessageIdAndPublicBadgeAndRemindAllCountAsync(Guid sessionId, long lastMessageId, DateTime messageCreationTime, Guid ignoreSessionUnitId, bool isRemindAll);

        Task<int> IncrementRemindMeCountAsync(DateTime messageCreationTime, List<Guid> sessionUnitIdList);

        Task<int> IncrementFollowingCountAsync(Guid sessionId, DateTime messageCreationTime, List<Guid> ownerSessionUnitIdList);

        Task<int> BatchUpdateNameAsync(long chatObjectId, string name, string nameSpelling, string nameSpellingAbbreviation);

        Task<int> BatchUpdateAppUserIdAsync(long chatObjectId, Guid appUserId);
    }
}
