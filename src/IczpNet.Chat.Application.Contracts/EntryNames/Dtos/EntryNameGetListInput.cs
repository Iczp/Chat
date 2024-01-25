using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.EntryNames.Dtos;

public class EntryNameGetListInput : BaseTreeGetListInput<Guid>
{
    /// <summary>
    /// 是否选择
    /// </summary>
    public virtual string InputType { get; set; }
    /// <summary>
    /// 是否选择
    /// </summary>
    public virtual bool? IsChoice { get; set; }

    /// <summary>
    /// 是否唯一
    /// </summary>
    public virtual bool? IsUniqued { get; set; }

    /// <summary>
    /// 是否必填
    /// </summary>
    public virtual bool? IsRequired { get; set; }

    /// <summary>
    /// 是否固定
    /// </summary>
    public virtual bool? IsStatic { get; set; }

    /// <summary>
    /// 是否公开
    /// </summary>
    public virtual bool? IsPublic { get; set; }
}
