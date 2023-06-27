using IczpNet.Chat.ChatObjects;
using System.Threading.Tasks;
using System;
using IczpNet.Chat.SessionUnits;

namespace IczpNet.Chat.OfficialSections.Officials
{
    public interface IOfficialManager
    {
        Task<SessionUnit> SubscribeAsync(long ownerId, long destinationId);

        Task<SessionUnit> SubscribeByIdAsync(Guid sessionUnitId);

        Task<SessionUnit> UnsubscribeAsync(Guid sessionUnitId);
    }
}
