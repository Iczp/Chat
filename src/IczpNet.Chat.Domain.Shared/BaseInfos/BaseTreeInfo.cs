using IczpNet.AbpTrees;
using System;

namespace IczpNet.Chat.BaseInfos
{
    public class BaseTreeInfo : TreeInfo<Guid>
    {
        //public virtual string Code { get; set; }

        public virtual double Sorting { get; set; }

        //public virtual bool IsStatic { get; set; }

        //public virtual bool IsActive { get; set; }
    }
}
