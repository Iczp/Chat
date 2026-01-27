using System;

namespace IczpNet.Chat.ConnectionPools;

public class LastOnline
{
    public long OwnerId { get; set; }   
    public string DeviceType { get; set; }
    public string DeviceId { get; set; }
    public DateTime? LastTime { get; set; }
}
