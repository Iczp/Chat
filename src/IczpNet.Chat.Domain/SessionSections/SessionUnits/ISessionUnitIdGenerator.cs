using System;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public interface ISessionUnitIdGenerator
    {
        Guid Create(long ownerId, long destinationId);

        string Generate(long ownerId, long destinationId);

        long[] Resolving(string sessionUnitId);
    }
}
