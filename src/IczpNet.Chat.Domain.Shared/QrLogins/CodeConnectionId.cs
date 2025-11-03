using System;

namespace IczpNet.Chat.QrLogins;

public class GrantedInfo()
{
    public string ConnectionId { get; set; }

    public Guid UserId { get; set; } 

    public Guid QrLoginCode { get; set; }

    public DateTime ExpiredTime { get; set; }
}
