using IczpNet.AbpCommons.DataFilters;
using IczpNet.AbpTrees.Dtos;

namespace IczpNet.Chat.BaseDtos
{
    public class BaseTreeGetListInput<TKey> : TreeGetListInput<TKey>, IKeyword where TKey : struct
    {
        //[DefaultValue(null)]
        //public virtual bool? IsStatic { get; set; }

        //[DefaultValue(null)]
        //public virtual bool? IsActive { get; set; }
    }
}
