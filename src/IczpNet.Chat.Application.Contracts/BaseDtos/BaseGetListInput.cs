using IczpNet.AbpCommons.DataFilters;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.BaseDtos
{
    public class BaseGetListInput : PagedAndSortedResultRequestDto, IKeyword
    {
        /// <summary>
        /// 关键字(支持拼音)
        /// </summary>
        [DefaultValue(null)]
        public virtual string Keyword { get; set; }

        /// <summary>
        /// 显示数量
        /// </summary>
        [DefaultValue(10)]
        public override int MaxResultCount { get; set; }

        /// <summary>
        /// 跳过数量
        /// </summary>
        //[DefaultValue(0)]
        public override int SkipCount { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [DefaultValue(null)]
        public override string Sorting { get; set; }
    }
}
