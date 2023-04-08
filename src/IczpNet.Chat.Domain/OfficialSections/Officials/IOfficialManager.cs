using IczpNet.Chat.ChatObjects;
using System.Threading.Tasks;
using System;
using IczpNet.Chat.SessionSections.SessionUnits;

namespace IczpNet.Chat.OfficialSections.Officials
{
    public interface IOfficialManager : IChatObjectManager
    {
        Task<SessionUnit> EnableAsync(long ownerId, long destinationId);

        Task<SessionUnit> EnableAsync(Guid sessionUnitId);

        Task<SessionUnit> DisableAsync(Guid sessionUnitId);
    }
}
