using IczpNet.AbpCommons.DataFilters;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Sessions.Dtos
{
    public class SessionTagGetListInput : PagedAndSortedResultRequestDto, IKeyword
    {
        public virtual Guid SessionId { get; set; }

        public virtual string Keyword { get; set; }
    }
}
