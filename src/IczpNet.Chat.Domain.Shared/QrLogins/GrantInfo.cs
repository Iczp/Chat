using System;

namespace IczpNet.Chat.QrLogins;

public class CodeConnectionId(string connectionId)
{
    public string ConnectionId { get; set; } = connectionId;
}
