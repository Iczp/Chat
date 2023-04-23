using IczpNet.AbpCommons.DataFilters;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionRoles.Dtos
{
    public class SessionRoleGetListBySessionUnitInput : PagedAndSortedResultRequestDto, IKeyword
    {
        public virtual string Keyword { get; set; }
    }
}
