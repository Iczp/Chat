using System;

namespace IczpNet.Chat.ScanLogins;

public class GrantedInfo()
{
    public string ConnectionId { get; set; }

    public Guid UserId { get; set; } 

    public Guid LoginCode { get; set; }

    public DateTime ExpiredTime { get; set; }
}
