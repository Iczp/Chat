using IczpNet.AbpTrees.Dtos;
using System;
using System.ComponentModel;

namespace IczpNet.Chat.BaseDtos
{
    public class BaseTreeInputDto: ITreeInput<Guid>
    {
        [DefaultValue(null)]
        public virtual Guid? ParentId { get; set; }

        public virtual string Name { get; set; }

        //public virtual string Code { get; set; }

        //public virtual bool IsActive { get; set; }

        public virtual double Sorting { get; set; }
    }
}
