using IczpNet.AbpCommons.DataFilters;
using System.Collections.Generic;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos
{
    public class SessionPermissionDefinitionGetListInput : PagedAndSortedResultRequestDto, IKeyword
    {
        [DefaultValue(null)]
        public virtual List<long> GroupIdList { get; set; }
        public bool IsImportChildGroup { get; set; }

        public virtual string Keyword { get; set; }
    }
}
