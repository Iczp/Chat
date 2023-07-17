using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class Message
{
    /// <summary>
    /// 引用来源Id(引用才有)
    /// </summary>
    public virtual long? QuoteMessageId { get; protected set; }

    /// <summary>
    /// 引用层级 0:不是引用
    /// </summary>
    public virtual long QuoteDepth { get; protected set; }

    /// <summary>
    /// 引用层级
    /// </summary>
    [StringLength(QuotePathMaxLength)]
    public virtual string QuotePath { get; protected set; }

    /// <summary>
    /// 引用自...
    /// </summary>
    [ForeignKey(nameof(QuoteMessageId))]
    public virtual Message QuoteMessage { get; protected set; }

    /// <summary>
    /// 被引用列表
    /// </summary>
    [InverseProperty(nameof(QuoteMessage))]
    public virtual IList<Message> QuotedMessageList { get; protected set; }
}
