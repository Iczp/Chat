using System;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class SessionUnitIdGenerator : DomainService, ISessionUnitIdGenerator
    {
        public Guid Create(long ownerId, long destinationId)
        {
            return GuidGenerator.Create();
        }
    }
}
