using System;

namespace IczpNet.Chat.ConnectionPools.Dtos;

public class LatestDto
{
    public string DeviceId { get; set; }
    public string DeviceType { get; set; }
    public DateTime? Latest { get; set; }
}
