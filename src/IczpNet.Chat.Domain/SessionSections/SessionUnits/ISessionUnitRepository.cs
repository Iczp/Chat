using System.Threading.Tasks;
using System;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public interface ISessionUnitRepository : IRepository<SessionUnit, Guid>
    {

        Task<int> BatchUpdateAsync(Guid sessionId, long lastMessageAutoId);
    }
}
