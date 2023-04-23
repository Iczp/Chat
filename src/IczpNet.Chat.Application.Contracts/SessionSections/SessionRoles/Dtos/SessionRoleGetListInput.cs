using IczpNet.AbpCommons.DataFilters;
using System;

namespace IczpNet.Chat.SessionSections.SessionRoles.Dtos
{
    public class SessionRoleGetListInput : SessionRoleGetListBySessionUnitInput, IKeyword, ISessionId
    {
        public virtual Guid? SessionId { get; set; }

        //public virtual string Keyword { get; set; }
    }
}
