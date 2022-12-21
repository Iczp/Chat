using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Sessions.Dtos
{
    public class SessionGetListInput : PagedAndSortedResultRequestDto
    {
        public virtual Guid? OwnerId { get; set; }
    }
}