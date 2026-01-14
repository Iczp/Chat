using System;

namespace IczpNet.Chat.ConnectionPools;

public class OnlinePeakInfo
{
    public long Count { get; set; }
    public DateTimeOffset Time { get; set; }
    public string Host { get; set; } = default!;
    public string Reason { get; set; } = default!;
}