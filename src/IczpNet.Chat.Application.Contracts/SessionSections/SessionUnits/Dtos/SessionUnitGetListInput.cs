using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionUnitGetListInput : PagedAndSortedResultRequestDto
    {
        public virtual Guid? OwnerId { get; set; }

        public virtual Guid? DestinationId { get;  set; }

        public virtual bool? IsKilled { get; set; }

        public virtual JoinWays? JoinWay { get; set; }

        public virtual Guid? InviterId { get; set; }
    }
}