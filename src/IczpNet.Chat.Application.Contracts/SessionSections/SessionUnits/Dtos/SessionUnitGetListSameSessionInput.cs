using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitGetListSameSessionInput : PagedAndSortedResultRequestDto, IKeyword
    {
        public virtual long SourceId { get; set; }

        public virtual long TargetId { get; set; }

        [DefaultValue(null)]
        public virtual List<ChatObjectTypeEnums> ObjectTypeList { get; set; }

        public virtual string Keyword { get; set; }
    }
}