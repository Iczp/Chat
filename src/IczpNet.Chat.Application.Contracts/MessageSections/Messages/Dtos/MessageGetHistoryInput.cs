using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageGetHistoryInput
{
    /// <summary>
    /// 会话单元Id
    /// </summary>
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual int MaxResultCount { get; set; } = 20;

    /// <summary>
    /// 最小消息Id
    /// </summary>
    public virtual long? MaxMessageId { get; set; }

    /// <summary>Read the committed SQL view for a V2 baseline.</summary>
    public bool Authoritative { get; set; }

    /// <summary>是否在 extra.senders 中以去重字典返回发送人详情，并置空每条消息内的重复 SenderSessionUnit。</summary>
    public bool CompactSenders { get; set; }
}
