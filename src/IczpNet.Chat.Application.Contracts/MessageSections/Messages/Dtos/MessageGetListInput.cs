using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageGetListInput : GetListInput
{
    /// <summary>
    /// 会话单元Id
    /// </summary>
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    /// <summary>
    /// 发送人【聊天对象】
    /// </summary>
    public virtual long? SenderId { get; set; }

    /// <summary>
    /// 是否有提醒
    /// </summary>
    public virtual bool? IsRemind { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public virtual List<MessageTypes> MessageTypes { get; set; }

    /// <summary>
    /// 是否特别关注
    /// </summary>
    public virtual bool? IsFollowed { get; set; }

    /// <summary>
    /// 转发层级
    /// </summary>
    public virtual int? ForwardDepth { get; set; }

    /// <summary>
    /// 引用层级
    /// </summary>
    public virtual int? QuoteDepth { get; set; }

    /// <summary>
    /// 最小消息Id
    /// </summary>
    public virtual long? MinMessageId { get; set; }

    /// <summary>
    /// 最大消息Id
    /// </summary>
    public virtual long? MaxMessageId { get; set; }

    /// <summary>
    /// 起始时间(包含)
    /// </summary>
    public virtual DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间(不包含)
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
    /// 允许的扩展名列表
    /// </summary>
    public virtual List<string> Extensions { get; set; }

    /// <summary>
    /// 是否在 extra.senders 中以去重字典返回发送人详情，并置空每条消息内的重复 SenderSessionUnit
    /// </summary>
    public virtual bool CompactSenders { get; set; }
}
