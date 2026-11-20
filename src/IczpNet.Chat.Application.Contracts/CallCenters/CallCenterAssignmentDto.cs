using System;

namespace IczpNet.Chat.CallCenters;

public class CallCenterAssignmentDto
{
    public Guid SessionId { get; set; }
    public long ShopKeeperId { get; set; }
    public long CustomerId { get; set; }
    public long? AgentId { get; set; }
    public CallCenterAssignmentStatus Status { get; set; }
    public DateTime QueuedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
}
