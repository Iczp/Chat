using IczpNet.AbpCommons.DataFilters;
using IczpNet.AbpTrees;
using System;
using Volo.Abp.MultiTenancy;

namespace IczpNet.Chat.BaseEntitys
{
    public abstract class BaseTreeEntity<T, TKey> : TreeEntity<T, TKey>, IMultiTenant, ISorting
        where T : ITreeEntity<TKey>
        where TKey : struct
    {
        public virtual Guid? TenantId { get; set; }
    }
}
