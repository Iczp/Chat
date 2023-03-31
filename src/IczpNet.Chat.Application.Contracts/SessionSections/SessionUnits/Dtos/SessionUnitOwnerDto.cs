using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.SessionTagDtos.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitOwnerDto
    {
        public virtual Guid Id { get; set; }

        public virtual string Rename { get; set; }

        public virtual Guid SessionId { get; set; }

        public virtual ChatObjectDto Owner { get; set; }

        public virtual long? InviterId { get; set; }

        public virtual JoinWays? JoinWay { get; set; }

        public virtual List<SessionRoleDto> RoleList { get; set; }

        public virtual List<SessionTagDto> TagList { get; set; }

    }
}
