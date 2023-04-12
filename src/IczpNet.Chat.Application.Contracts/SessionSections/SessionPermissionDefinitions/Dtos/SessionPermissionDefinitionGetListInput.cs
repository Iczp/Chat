using IczpNet.AbpCommons.DataFilters;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos
{
    public class SessionPermissionDefinitionGetListInput : PagedAndSortedResultRequestDto, IKeyword
    {
        public virtual string Keyword { get; set; }
    }
}
