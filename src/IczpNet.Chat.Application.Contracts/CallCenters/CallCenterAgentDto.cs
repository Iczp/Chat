using IczpNet.Chat.Enums;

namespace IczpNet.Chat.CallCenters;

public class CallCenterAgentDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public ServiceStatus? ServiceStatus { get; set; }
    public int ActiveCount { get; set; }
}
