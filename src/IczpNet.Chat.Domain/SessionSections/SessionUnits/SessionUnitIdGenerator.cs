using IczpNet.Chat.TextTemplates;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class SessionUnitIdGenerator : DomainService, ISessionUnitIdGenerator
    {
        public Guid Create(long ownerId, long destinationId)
        {
            var id = string.Format("{0}_{1}", IntStringHelper.IntToString(ownerId), IntStringHelper.IntToString(destinationId));
            return GuidGenerator.Create();
        }
    }
}
