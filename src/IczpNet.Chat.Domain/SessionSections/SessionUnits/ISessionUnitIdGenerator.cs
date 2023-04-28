using System;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public interface ISessionUnitIdGenerator
    {
        Guid Create(long ownerId, long destinationId);

        Guid Resolve(string sessionUnitId);
    }
}
