using System;

namespace IczpNet.Chat.ScanLogins;

public class CodeConnectionId(string connectionId)
{
    public string ConnectionId { get; set; } = connectionId;
}
