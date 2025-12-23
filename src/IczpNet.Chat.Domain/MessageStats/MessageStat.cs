using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Sessions;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageStats;

/// <summary>
/// 
/// </summary>

public class MessageStat : BaseEntity<Guid>
{

    /// <summary>
    /// 
    /// </summary>
    [Comment("会话Id")]
    public virtual Guid SessionId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Comment("消息类型")]
    public virtual MessageTypes MessageType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Comment("日期(数字)")]
    public virtual long DateBucket { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Comment("数量")]
    public virtual long Count { get; set; }

    protected MessageStat() { }

    public MessageStat(Guid id) : base(id) { }

}
