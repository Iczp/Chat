using System;
using System.Collections.Generic;

namespace IczpNet.Chat.CallCenters;

public class CallCenterAssignmentPageDto
{
    public List<CallCenterAssignmentDto> Items { get; set; } = [];
    public DateTime? NextQueuedAt { get; set; }
    public Guid? NextId { get; set; }
}
