using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionRoles.Dtos
{
    public class SessionRoleDto : EntityDto<Guid>, ISessionId
    {
        public virtual Guid? SessionId { get; set; }

        public virtual string Name { get; set; }

        public virtual int PermissionCount { get; set; }

        public virtual bool IsDefault { get; set; }

        public virtual string Description { get; set; }
    }
}
