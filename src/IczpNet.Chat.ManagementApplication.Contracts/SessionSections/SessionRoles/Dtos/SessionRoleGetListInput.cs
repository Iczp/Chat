using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.SessionSections;
using System;

namespace IczpNet.Chat.Management.SessionSections.SessionRoles.Dtos
{
    public class SessionRoleGetListInput : SessionRoleGetListBySessionUnitInput, IKeyword, ISessionId
    {
        public virtual Guid? SessionId { get; set; }

        //public virtual string Keyword { get; set; }
    }
}
