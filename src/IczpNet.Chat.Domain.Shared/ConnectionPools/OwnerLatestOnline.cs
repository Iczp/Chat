using System;

namespace IczpNet.Chat.ConnectionPools;

public class OwnerLatestOnline
{
    public string DeviceType { get; set; }
    public string DeviceId { get; set; }
    public DateTime? LatestTime { get; set; }
}
