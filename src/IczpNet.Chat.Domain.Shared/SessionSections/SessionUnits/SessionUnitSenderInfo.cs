using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.SessionTags;
using System;
using System.Collections.Generic;
using Volo.Abp.ObjectExtending;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public class SessionUnitSenderInfo : ExtensibleObject
{
    public virtual Guid Id { get; set; }

    /// <summary>
    /// 聊天对象Id
    /// </summary>
    public virtual long OwnerId { get; set; }

    /// <summary>
    /// 对象类型
    /// </summary>
    public virtual ChatObjectTypeEnums? OwnerObjectType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual string DisplayName { get; set; }

    /// <summary>
    /// 会话内名称（如：群内名称）
    /// </summary>
    public virtual string MemberName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime CreationTime { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectInfo Owner { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual List<SessionTagInfo> TagList { get; set; }
}
