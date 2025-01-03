﻿using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.Templates;

public class HistoryMessage : BaseEntity
{
    public virtual Guid HistoryContentId { set; get; }

    [ForeignKey(nameof(HistoryContentId))]
    public virtual HistoryContent HistoryContent { set; get; }

    public virtual long MessageId { set; get; }

    [ForeignKey(nameof(MessageId))]
    public virtual Message Message { set; get; }

    public override object[] GetKeys()
    {
        return [MessageId, HistoryContentId];
    }

    protected HistoryMessage() { }
    public HistoryMessage(HistoryContent historyContent, Message message)
    {

        HistoryContent = historyContent;
        Message = message;
    }
}
