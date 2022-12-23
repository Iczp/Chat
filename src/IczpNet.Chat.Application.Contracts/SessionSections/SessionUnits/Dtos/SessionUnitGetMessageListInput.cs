using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitGetMessageListInput : PagedAndSortedResultRequestDto
    {
        public virtual Guid? SenderId { get; set; }

        public virtual bool? IsRemind { get; set; }

        public virtual long? MinAutoId { get; set; }

        public virtual long? MaxAutoId { get; set; }
    }
}
