using IczpNet.Chat.Enums;
using System;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitGetListInput : PagedAndSortedResultRequestDto
    {
        public virtual Guid? OwnerId { get; set; }

        public virtual Guid? DestinationId { get; set; }

        public virtual bool? IsKilled { get; set; }

        public virtual ChatObjectTypes? DestinationObjectType { get; set; }

        [DefaultValue(false)]
        public virtual bool IsRemind { get; set; }

        [DefaultValue(false)]
        public virtual bool IsBadge { get; set; }

        public virtual long? MinAutoId { get; set; }

        public virtual long? MaxAutoId { get; set; }

        //public virtual JoinWays? JoinWay { get; set; }

        //public virtual Guid? InviterId { get; set; }

        public virtual bool IsOrderByBadge { get; set; }
    }
}