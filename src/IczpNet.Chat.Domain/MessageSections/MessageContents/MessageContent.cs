using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.MessageContents;

public class MessageContent :BaseEntity<Guid>
{
    [StringLength(50)]
    public virtual string  ContentType { get; set; }

    [StringLength(5000)]
    public virtual string Body { get; set; }

    public virtual List<Message> MessageList { get; set; }
}
