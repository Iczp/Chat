using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace IczpNet.Chat.MessageSections.Messages;

/// <summary>Shared message change log (Edit / Recall) scoped by SessionId, supporting soft-delete and full auditing.</summary>
public class MessageChange : FullAuditedEntity<long>
{
    public override long Id { get; protected set; }

    public virtual Guid SessionId { get; protected set; }

    public virtual long MessageId { get; protected set; }

    public virtual MessageChangeKind ChangeType { get; protected set; }

    protected MessageChange() { }

    public MessageChange(Guid sessionId, long messageId, MessageChangeKind changeType, DateTime creationTime)
    {
        SessionId = sessionId;
        MessageId = messageId;
        ChangeType = changeType;
        CreationTime = creationTime;
    }
}
