using System;

namespace IczpNet.Chat.ScanLogins;

public class GrantedInfo()
{
    public string ConnectionId { get; set; }

    public Guid UserId { get; set; }

    public string UserName { get; set; }

    public Guid ScanToken { get; set; }

    public DateTime ExpiredTime { get; set; }
}
