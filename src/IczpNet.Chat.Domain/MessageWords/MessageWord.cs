using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Words;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageWords;

/// <summary>
/// 消息关键字
/// </summary>
public class MessageWord : BaseEntity
{
    /// <summary>
    /// 消息Id
    /// </summary>
    public virtual long MessageId { get; protected set; }

    /// <summary>
    /// 消息
    /// </summary>
    [ForeignKey(nameof(MessageId))]
    public virtual Message Message { get; protected set; }

    /// <summary>
    /// 关键字Id
    /// </summary>
    public virtual Guid WordId { get; protected set; }

    /// <summary>
    /// 关键字
    /// </summary>
    [ForeignKey(nameof(WordId))]
    public virtual Word Word { get; protected set; }

    protected MessageWord() { }

    public override object[] GetKeys()
    {
        return [MessageId, WordId];
    }
}
