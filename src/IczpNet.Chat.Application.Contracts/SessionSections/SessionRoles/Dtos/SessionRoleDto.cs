using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionRoles.Dtos
{
    public class SessionRoleDto : EntityDto<Guid>
    {
        public virtual string Name { get; set; }

        public virtual bool IsDefault { get; set; }

        public virtual string Description { get; set; }
    }
}
