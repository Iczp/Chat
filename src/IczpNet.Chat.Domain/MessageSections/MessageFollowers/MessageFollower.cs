using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.MessageFollowers;

/// <summary>
/// 消息关注者
/// </summary>
[Comment("消息关注者")]
public class MessageFollower : BaseEntity
{
    /// <summary>
    /// 是否已读(知晓)
    /// </summary>
    public virtual bool IsRead { get; set; }
    /// <summary>
    /// 消息Id
    /// </summary>
    public virtual long MessageId { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [ForeignKey(nameof(MessageId))]
    public virtual Message Message { get; set; }

    /// <summary>
    /// 发送人会话单元ID
    /// </summary>
    public virtual Guid SessionUnitId { get; set; }

    /// <summary>
    /// 发送人会话单元
    /// </summary>
    [ForeignKey(nameof(SessionUnitId))]
    public virtual SessionUnit SessionUnit { get; set; }

    public override object[] GetKeys()
    {
        return [MessageId, SessionUnitId];
    }
}
