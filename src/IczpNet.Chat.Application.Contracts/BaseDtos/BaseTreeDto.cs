using IczpNet.AbpTrees.Dtos;
using System;
using System.ComponentModel;

namespace IczpNet.Chat.BaseDtos
{
    public class BaseTreeInputDto<TKey> : ITreeInput<TKey> where TKey : struct
    {
        [DefaultValue(null)]
        public virtual TKey? ParentId { get; set; }

        public virtual string Name { get; set; }

        //public virtual string Code { get; set; }

        //public virtual bool IsActive { get; set; }

        public virtual double Sorting { get; set; }

    }
}
