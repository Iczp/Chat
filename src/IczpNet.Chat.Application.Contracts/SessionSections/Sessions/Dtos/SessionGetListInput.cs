using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Sessions.Dtos
{
    public class SessionGetListInput : PagedAndSortedResultRequestDto
    {
        public virtual long? OwnerId { get; set; }
    }
}