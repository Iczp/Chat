using System;

namespace IczpNet.Chat.ScanLogins;

public class GenerateInfo
{
    public string ConnectionId { get; set; }

    public DateTime? ExpiredTime { get; set; }

    public string ScanText { get; set; }

    public Guid? ScanUserId { get; set; }

    public string ScanUserName { get; set; }

    public string ScanClientId { get; set; }

    public DateTime? ScanTime { get; set; }

    public string State { get; set; }

    public override string ToString()
    {
        return $"{nameof(ConnectionId)}={ConnectionId},{nameof(State)}={State}";
    }
}
