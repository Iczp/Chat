using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionSections.SessionUnits;

[Serializable]
public class SessionUnitInfoBase //: ExtensibleObject
{
    /// <summary>
    /// SessionUnitId
    /// </summary>
    public virtual Guid Id { get; set; }

    /// <summary>
    /// 会话Id
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// OwnerId
    /// </summary>
    public virtual long OwnerId { get; set; }

    /// <summary>
    /// OwnerType
    /// </summary>
    public virtual ChatObjectTypeEnums? OwnerObjectType { get; set; }

    /// <summary>
    /// 备注名称
    /// </summary>
    public virtual string Rename { get; set; }

    /// <summary>
    /// 会话内名称
    /// </summary>
    public virtual string MemberName { get; set; }

    /// <summary>
    /// 是否固定成员
    /// </summary>
    public virtual bool IsStatic { get; set; }

    /// <summary>
    /// 是否公开
    /// </summary>
    public virtual bool IsPublic { get; set; }

    /// <summary>
    /// 是否可见
    /// </summary>
    public virtual bool IsVisible { get; set; }

    /// <summary>
    /// 是否可用
    /// </summary>
    public virtual bool IsEnabled { get; set; }

    /// <summary>
    /// 是否创建者
    /// </summary>
    public virtual bool IsCreator { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime CreationTime { get; set; }
}
