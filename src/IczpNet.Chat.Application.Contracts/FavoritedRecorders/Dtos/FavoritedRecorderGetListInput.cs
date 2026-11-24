using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.FavoritedRecorders.Dtos;

public class FavoritedRecorderGetListInput : GetListInput
{
    /// <summary>
    /// 所有聊天对象
    /// </summary>
    public long? OwnerId { get; set; }

    /// <summary>
    /// 目标聊天对象
    /// </summary>
    public long? DestinationId { get; set; }

    /// <summary>
    /// 会话单元Id
    /// </summary>
    public virtual Guid? SessionUnitId { get; set; }

    /// <summary>
    /// 会话Id
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// 发送人聊天对象Id
    /// </summary>
    public virtual long? SenderId { get; set; }

    /// <summary>
    /// 消息大小（最小值）
    /// </summary>
    public virtual long? MinSize { get; set; }

    /// <summary>
    /// 消息大小（最大值）
    /// </summary>
    public virtual long? MaxSize { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public virtual MessageTypes? MessageType { get; set; }

    /// <summary>
    /// 消息类型（多选）
    /// </summary>
    public virtual List<MessageTypes> MessageTypes { get; set; }

    /// <summary>
    /// 起始时间（包含）
    /// </summary>
    public virtual DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间（不包含）
    /// </summary>
    public virtual DateTime? EndTime { get; set; }

    /// <summary>
    /// 允许的扩展名列表（如 .jpg, .png, .pdf）
    /// </summary>
    public virtual List<string> Extensions { get; set; }
}
