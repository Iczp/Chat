using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.SessionTags.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitDestinationDto
    {
        public virtual Guid Id { get; set; }

        /// <summary>
        /// 会话内的名称
        /// </summary>
        [StringLength(50)]
        public virtual string DisplayName { get; set; }

        public virtual Guid SessionId { get; set; }

        public virtual ChatObjectDto Owner { get; set; }

        //public virtual long? InviterId { get; set; }

        //public virtual JoinWays? JoinWay { get; set; }

        //public virtual bool IsKilled { get; set; }

        //public virtual bool IsPublic { get; set; }

        //public virtual bool IsStatic { get; set; }

        //public virtual bool IsCreator { get; set; }

        public virtual List<SessionRoleDto> RoleList { get; set; }

        public virtual List<SessionTagDto> TagList { get; set; }

    }
}
