using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitGetMessageListInput : PagedAndSortedResultRequestDto, IKeyword
    {
        public virtual long? SenderId { get; set; }

        public virtual bool? IsRemind { get; set; }

        public virtual MessageTypes? MessageType { get; set; }

        public virtual bool? IsFollowed { get; set; }

        public virtual long? MinMessageId { get; set; }

        public virtual long? MaxMessageId { get; set; }

        public virtual string Keyword { get; set; }
    }
}
