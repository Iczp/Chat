using IczpNet.AbpCommons.DataFilters;
using IczpNet.AbpTrees.Dtos;
using System;
using System.ComponentModel;

namespace IczpNet.Chat.BaseDtos
{
    public class BaseTreeGetListInput : TreeGetListInput<Guid>, IKeyword
    {
        //[DefaultValue(null)]
        //public virtual bool? IsStatic { get; set; }

        //[DefaultValue(null)]
        //public virtual bool? IsActive { get; set; }
    }
}
