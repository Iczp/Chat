using IczpNet.Chat.SessionUnits;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.OfficialSections.Officials;

public interface IOfficialManager
{
    Task<SessionUnit> SubscribeAsync(long ownerId, long destinationId);

    Task<SessionUnit> SubscribeByIdAsync(Guid sessionUnitId);

    Task<SessionUnit> UnsubscribeAsync(Guid sessionUnitId);
}
