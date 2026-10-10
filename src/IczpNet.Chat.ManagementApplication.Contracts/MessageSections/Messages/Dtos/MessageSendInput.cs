using IczpNet.Chat.MessageSections;
using System;

namespace IczpNet.Chat.Management.MessageSections.Messages.Dtos;

public class MessageSendInput<T> : MessageInput<T>
{
    public virtual Guid SessionUnitId { get; set; }
}
