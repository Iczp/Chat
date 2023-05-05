using IczpNet.AbpCommons.DataFilters;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.OpenedRecorders.Dtos
{
    public class GetListByMessageIdInput : PagedAndSortedResultRequestDto, IKeyword
    {
        [DefaultValue(true)]
        public virtual bool IsReaded { get; set; }

        public virtual string Keyword { get; set; }
    }
}
