using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionUnitManager
    {

        Task<SessionUnit> SetReadedAsync(SessionUnit entity, Guid messageId);

        Task<SessionUnit> RemoveAsync(SessionUnit entity);

        Task<SessionUnit> KillAsync(SessionUnit entity);

        Task<SessionUnit> ClearAsync(SessionUnit entity);

        Task<SessionUnit> DeleteMessageAsync(SessionUnit entity, Guid messageId);
    }
}
