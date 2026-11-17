using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

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

/// <summary>Per-user, per-ChatObject, per-device shared message change sync cursor.</summary>
public class MessageChangeCursor : AggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid UserId { get; protected set; }

    public virtual long ChatObjectId { get; protected set; }

    public virtual string DeviceId { get; protected set; }

    public virtual long LastAppliedMessageChangeId { get; protected set; }

    public virtual bool IsInitialized { get; protected set; }

    public virtual DateTime? LastSyncTime { get; protected set; }

    protected MessageChangeCursor() { }

    public MessageChangeCursor(
        Guid id,
        Guid? tenantId,
        Guid userId,
        long chatObjectId,
        string deviceId,
        long lastAppliedMessageChangeId,
        bool isInitialized,
        DateTime? lastSyncTime) : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        ChatObjectId = chatObjectId;
        DeviceId = deviceId;
        LastAppliedMessageChangeId = lastAppliedMessageChangeId;
        IsInitialized = isInitialized;
        LastSyncTime = lastSyncTime;
    }

    public void Advance(long lastAppliedMessageChangeId, DateTime syncTime)
    {
        if (lastAppliedMessageChangeId > LastAppliedMessageChangeId)
            LastAppliedMessageChangeId = lastAppliedMessageChangeId;
        IsInitialized = true;
        LastSyncTime = syncTime;
    }

    public void Reset(long lastAppliedMessageChangeId, DateTime syncTime)
    {
        LastAppliedMessageChangeId = lastAppliedMessageChangeId;
        IsInitialized = true;
        LastSyncTime = syncTime;
    }
}

/// <summary>Global write gate serializing MessageChange IDENTITY commits.</summary>
public class MessageChangeWriteGate : Entity<int>
{
    public const int DefaultId = 1;
    public static readonly Guid InitialEpoch = new("11111111-1111-1111-1111-111111111111");

    public virtual long Revision { get; protected set; }

    public virtual Guid Epoch { get; protected set; }

    public virtual long LastCommittedChangeId { get; protected set; }

    protected MessageChangeWriteGate() { }

    public MessageChangeWriteGate(int id, Guid epoch, long revision = 0, long lastCommittedChangeId = 0) : base(id)
    {
        Epoch = epoch;
        Revision = revision;
        LastCommittedChangeId = lastCommittedChangeId;
    }
}

/// <summary>Serializes per-session private delete/clear commits without locking the hot Session row.</summary>
public class SessionMessageChangeState : Entity<Guid>
{
    public virtual Guid Epoch { get; protected set; }
    public virtual long Version { get; protected set; }
    public virtual long MinVersion { get; protected set; }
    protected SessionMessageChangeState() { }
}
