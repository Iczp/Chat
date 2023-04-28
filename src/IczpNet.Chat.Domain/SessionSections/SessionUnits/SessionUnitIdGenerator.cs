using IczpNet.Chat.TextTemplates;
using System;
using System.Linq;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class SessionUnitIdGenerator : DomainService, ISessionUnitIdGenerator
    {
        public virtual Guid Create(long ownerId, long destinationId)
        {
            var id = string.Format("{0}_{1}", IntStringHelper.IntToString(ownerId), IntStringHelper.IntToString(destinationId));
            return GuidGenerator.Create();
        }

        public virtual long[] Resolving(string sessionUnitId)
        {
            var arr = sessionUnitId.Split('_').Select(x => IntStringHelper.StringToInt(x)).ToArray();
            return arr; 
        }
    }
}
