using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Management.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitGetMessageListInput : PagedAndSortedResultRequestDto, IKeyword
    {
        public virtual long? SenderId { get; set; }

        public virtual MessageTypes? MessageType { get; set; }

        public virtual bool? IsRemind { get; set; }

        public virtual long? MinAutoId { get; set; }

        public virtual long? MaxAutoId { get; set; }

        public virtual string Keyword { get; set; }
    }
}
