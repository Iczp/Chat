using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace IczpNet.Chat.MessageSections.Messages;

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
