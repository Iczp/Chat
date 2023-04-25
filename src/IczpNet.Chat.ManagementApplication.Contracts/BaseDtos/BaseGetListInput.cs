using IczpNet.AbpCommons.DataFilters;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Management.BaseDtos
{
    public class BaseGetListInput : PagedAndSortedResultRequestDto, IKeyword
    {
        /// <summary>
        /// 关键字(支持拼音)
        /// </summary>
        [DefaultValue(null)]
        public virtual string Keyword { get; set; }
    }
}
