using System;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.MessageSections.Messages;

public class MessageChange : Entity<Guid>
{
    public virtual Guid SessionId { get; protected set; }
    public virtual long MessageId { get; protected set; }
    public virtual long Version { get; protected set; }
    public virtual MessageChangeKind ChangeType { get; protected set; }
    public virtual DateTime ChangedAt { get; protected set; }
    public virtual DateTime MessageCreationTime { get; protected set; }
    public virtual bool IsPrivate { get; protected set; }
    public virtual Guid? SenderSessionUnitId { get; protected set; }
    public virtual Guid? ReceiverSessionUnitId { get; protected set; }
    protected MessageChange() { }
    public MessageChange(Guid id, Message message, long version, DateTime time) : base(id)
    {
        SessionId = message.SessionId!.Value;
        MessageId = message.Id;
        Version = version;
        ChangeType = MessageChangeKind.Rollback;
        ChangedAt = time;
        MessageCreationTime = message.CreationTime;
        IsPrivate = message.IsPrivate;
        SenderSessionUnitId = message.SenderSessionUnitId;
        ReceiverSessionUnitId = message.ReceiverSessionUnitId;
    }

    // For Delete, ReceiverSessionUnitId is the affected member, not the
    // original message recipient. This scope survives without a source row.
    public MessageChange(Guid id, Message message, long version, DateTime time, Guid deletedFor)
        : this(id, message, version, time)
    {
        ChangeType = MessageChangeKind.Delete;
        ReceiverSessionUnitId = deletedFor;
    }
}

/// <summary>Serializes change commits without locking the hot Session row.</summary>
public class SessionMessageChangeState : Entity<Guid>
{
    public virtual Guid Epoch { get; protected set; }
    public virtual long Version { get; protected set; }
    public virtual long MinVersion { get; protected set; }
    protected SessionMessageChangeState() { }
}
