using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Sessions.Dtos
{
    public class SessionMessageGetListInput : PagedAndSortedResultRequestDto
    {
        public virtual Guid SessionId { get; set; }

        public virtual Guid? SenderId { get; set; }

        public virtual long? MinAutoId { get; set; }

        public virtual long? MaxAutoId { get; set; }
    }
}
