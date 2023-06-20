using IczpNet.AbpCommons.DataFilters;
using IczpNet.AbpTrees.Dtos;
using System.Collections.Generic;
using System.ComponentModel;

namespace IczpNet.Chat.BaseDtos
{
    public class BaseTreeGetListInput<TKey> : TreeGetListInput<TKey>, IKeyword where TKey : struct
    {
        /// <summary>
        /// 是否启用 ParentId
        /// </summary>
        [DefaultValue(false)]
        public override bool IsEnabledParentId { get; set; }

        /// <summary>
        /// 父级Id,当IsEnabledParentId=false时,查询全部
        /// </summary>
        [DefaultValue(null)]
        public override TKey? ParentId { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        [DefaultValue(null)]
        public override List<int> DepthList { get; set; }

        //[DefaultValue(null)]
        //public virtual bool? IsStatic { get; set; }

        //[DefaultValue(null)]
        //public virtual bool? IsActive { get; set; }

        /// <summary>
        /// 关键字(支持拼音)
        /// </summary>
        [DefaultValue(null)]
        public override string Keyword { get; set; }

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

        public override string ToString()
        {
            return $"[TreeGetListInput: {GetType().Name}] ParentId = {ParentId}";
        }
    }
}
