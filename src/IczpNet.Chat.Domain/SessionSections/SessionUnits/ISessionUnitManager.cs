using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionUnitManager
    {
        Task<SessionUnit> SetToppingAsync(SessionUnit entity, bool isTopping);

        Task<SessionUnit> SetReadedAsync(SessionUnit entity, Guid messageId, bool isForce = false);

        Task<SessionUnit> RemoveSessionAsync(SessionUnit entity);

        Task<SessionUnit> KillSessionAsync(SessionUnit entity);

        Task<SessionUnit> ClearMessageAsync(SessionUnit entity);

        Task<SessionUnit> DeleteMessageAsync(SessionUnit entity, Guid messageId);

        Task<int> GetBadgeAsync(Guid ownerId);

        Task<int> GetCountAsync(Guid sessionId);

        Task<int> BatchUpdateAsync(Guid sessionId, long lastMessageAutoId);
    }
}
