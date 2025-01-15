using IczpNet.AbpTrees.Dtos;
using System.ComponentModel;

namespace IczpNet.Chat.BaseDtos;

public class BaseTreeInputDto<TKey> : ITreeInput<TKey> where TKey : struct
{
    /// <summary>
    /// 父级Id,为null时，根级
    /// </summary>
    [DefaultValue(null)]
    public virtual TKey? ParentId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public virtual string Name { get; set; }

    //public virtual string Code { get; set; }

    //public virtual bool IsActive { get; set; }

    /// <summary>
    /// 排序(越大越前)
    /// </summary>
    public virtual double Sorting { get; set; }

}
