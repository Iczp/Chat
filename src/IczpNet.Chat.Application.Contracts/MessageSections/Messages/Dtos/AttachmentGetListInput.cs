using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

/// <summary>
/// 跨会话/单会话统一附件检索输入
/// </summary>
public class AttachmentGetListInput : GetListInput
{
    /// <summary>
    /// 所属聊天对象Id（可选，不传则按当前登录用户所有身份）
    /// </summary>
    public virtual long? OwnerId { get; set; }

    /// <summary>
    /// 会话单元Id（可选，传入时限定该会话单元）
    /// </summary>
    public virtual Guid? SessionUnitId { get; set; }

    /// <summary>
    /// 会话Id（可选，传入时限定该会话）
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// 发送人聊天对象Id
    /// </summary>
    public virtual long? SenderId { get; set; }

    /// <summary>
    /// 媒体/消息类型（不传则默认检索图片、视频、语音、文件、链接）
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
    /// 最小大小（字节）
    /// </summary>
    public virtual long? MinSize { get; set; }

    /// <summary>
    /// 最大大小（字节）
    /// </summary>
    public virtual long? MaxSize { get; set; }

    /// <summary>
    /// 扩展名过滤（如 jpg, png, pdf）
    /// </summary>
    public virtual List<string> Extensions { get; set; }
}
