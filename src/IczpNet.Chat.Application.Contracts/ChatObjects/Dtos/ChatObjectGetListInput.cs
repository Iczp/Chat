using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace IczpNet.Chat.ChatObjects.Dtos;

public class ChatObjectGetListInput : BaseTreeGetListInput<long>
{
    /// <summary>
    /// 聊天对象类型Id
    /// </summary>
    public virtual string ChatObjectTypeId { get; set; }

    /// <summary>
    /// 是否固定
    /// </summary>
    public virtual bool? IsStatic { get; set; }

    /// <summary>
    /// 是否公开
    /// </summary>
    public virtual bool? IsPublic { get; set; }

    /// <summary>
    /// 是否可用
    /// </summary>
    public virtual bool? IsEnabled { get; set; }

    /// <summary>
    /// 是否默认
    /// </summary>
    public virtual bool? IsDefault { get; set; }

    /// <summary>
    /// 聊天对象类型:个人|群|服务号等
    /// </summary>
    public virtual ChatObjectTypeEnums? ObjectType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual List<ChatObjectTypeEnums> ObjectTypes { get; set; }

    [DefaultValue(null)]
    public virtual List<Guid> CategoryIdList { get; set; }

    /// <summary>
    /// 包含下级
    /// </summary>
    [DefaultValue(null)]
    public bool? IsImportChildCategory { get; set; }
    
}
