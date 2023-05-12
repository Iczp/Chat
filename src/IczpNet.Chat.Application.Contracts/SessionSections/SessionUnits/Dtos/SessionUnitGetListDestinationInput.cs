using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitGetListDestinationInput : BaseGetListInput
    {
        //[Required]
        //public virtual Guid SessionId { get; set; }

        public virtual bool? IsKilled { get; set; }

        public virtual bool? IsPublic { get; set; }

        public virtual bool? IsStatic { get; set; }

        public virtual List<long> OwnerIdList { get; set; }

        [DefaultValue(null)]
        public virtual List<ChatObjectTypeEnums> OwnerTypeList { get; set; }

        public virtual Guid? TagId { get; set; }

        public virtual Guid? RoleId { get; set; }

        public virtual JoinWays? JoinWay { get; set; }

        public virtual long? InviterId { get; set; }

        public virtual Guid? InviterUnitId { get; set; }

    }
}