using System;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.MessageSections.Messages;

/// <summary>Append-only shared message change log (Edit / Recall) scoped by SessionId.</summary>
public class MessageChange : Entity<long>
{
    public override long Id { get; protected set; }

    public virtual Guid SessionId { get; protected set; }

    public virtual long MessageId { get; protected set; }

    public virtual MessageChangeKind ChangeType { get; protected set; }

    public virtual DateTime CreationTime { get; protected set; }

    protected MessageChange() { }

    public MessageChange(Guid sessionId, long messageId, MessageChangeKind changeType, DateTime creationTime)
    {
        SessionId = sessionId;
        MessageId = messageId;
        ChangeType = changeType;
        CreationTime = creationTime;
    }
}
