using System.Threading.Tasks;
using System;
using Volo.Abp.Domain.Repositories;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public interface ISessionUnitRepository : IRepository<SessionUnit, Guid>
    {

        Task<int> BatchUpdateAsync(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null);

        Task<Dictionary<Guid, SessionUnitStatModel>> GetStatsAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);
    }
}
