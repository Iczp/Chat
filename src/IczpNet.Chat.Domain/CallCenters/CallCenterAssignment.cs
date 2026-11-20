using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;

namespace IczpNet.Chat.CallCenters;

/// <summary>Current assignee or durable queue position for one customer session.</summary>
[Index(nameof(SessionId), IsUnique = true)]
[Index(nameof(ShopKeeperId), nameof(Status), nameof(QueuedAt), nameof(Id))]
[Index(nameof(ShopKeeperId), nameof(AgentId), nameof(Status), nameof(LastActivityAt))]
public class CallCenterAssignment : BaseEntity<Guid>
{
    public long ShopKeeperId { get; private set; }
    public Guid SessionId { get; private set; }
    public long CustomerId { get; private set; }
    public long? AgentId { get; private set; }
    public CallCenterAssignmentStatus Status { get; private set; }
    public DateTime QueuedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public long LastActivityMessageId { get; private set; }

    protected CallCenterAssignment() { }

    public CallCenterAssignment(Guid id, long shopKeeperId, Guid sessionId, long customerId, DateTime now) : base(id)
    {
        if (shopKeeperId <= 0 || customerId <= 0 || sessionId == Guid.Empty)
            throw new ArgumentException("Invalid customer service session.");
        ShopKeeperId = shopKeeperId;
        SessionId = sessionId;
        CustomerId = customerId;
        QueuedAt = now;
        LastActivityAt = now;
        Status = CallCenterAssignmentStatus.Queued;
    }

    public void Assign(long agentId)
    {
        if (agentId <= 0) throw new ArgumentOutOfRangeException(nameof(agentId));
        AgentId = agentId;
        Status = CallCenterAssignmentStatus.Assigned;
    }

    public void Queue(DateTime now)
    {
        AgentId = null;
        Status = CallCenterAssignmentStatus.Queued;
        QueuedAt = now;
    }

    public void RecordActivity(long messageId, DateTime time)
    {
        if (messageId <= 0) throw new ArgumentOutOfRangeException(nameof(messageId));
        if (messageId > LastActivityMessageId) LastActivityMessageId = messageId;
        if (time > LastActivityAt) LastActivityAt = time;
    }

    /// <summary>Only assigned conversations within the activity window consume capacity.</summary>
    public bool IsActive(DateTime now, TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        return Status == CallCenterAssignmentStatus.Assigned && AgentId.HasValue && LastActivityAt > now.Subtract(timeout);
    }
}
