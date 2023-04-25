using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Management.SessionSections.Sessions.Dtos
{
    public class SessionGetMessageListInput : PagedAndSortedResultRequestDto
    {
        //public virtual Guid SessionId { get; set; }

        public virtual long? SenderId { get; set; }

        public virtual long? MinAutoId { get; set; }

        public virtual long? MaxAutoId { get; set; }
    }
}
