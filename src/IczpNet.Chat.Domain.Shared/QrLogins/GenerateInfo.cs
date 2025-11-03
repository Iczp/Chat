using System;

namespace IczpNet.Chat.QrLogins;

public class GenerateInfo
{
    public string ConnectionId { get; set; }

    public DateTime? ExpiredTime { get; set; }

    public string QrText { get; set; }

    public Guid? ScanUserId { get; set; }
}
