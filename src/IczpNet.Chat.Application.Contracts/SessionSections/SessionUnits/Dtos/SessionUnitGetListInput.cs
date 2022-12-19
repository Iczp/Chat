using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionUnitGetListInput : PagedAndSortedResultRequestDto
    {
        public virtual Guid? OwnerId { get; set; }

        public virtual Guid? DestinationId { get; protected set; }
    }
}