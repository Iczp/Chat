using IczpNet.Chat.ConnectionPools;
using System;

namespace IczpNet.Chat.ScanLogins;

public class ScannedDto : GenerateInfo
{
    public ConnectionPool ConnectionPool { get; set; }
}
