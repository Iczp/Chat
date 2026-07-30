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
}
