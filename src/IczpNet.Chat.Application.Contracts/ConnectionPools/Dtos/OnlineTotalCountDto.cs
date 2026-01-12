using System;

namespace IczpNet.Chat.ConnectionPools.Dtos;

public class OnlineHostDto
{
    public string Host { get; set; }
    public long Count { get; set; }
    public DateTime? StartTime { get; set; }
}
