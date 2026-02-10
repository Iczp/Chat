using System;
using System.Collections.Generic;

namespace IczpNet.Chat.ConnectionPools;

public class OnlineFriendInfo
{
    public long OwnerId { get; set; }
    public long DestinationId { get; set; }
    public Guid SessionId { get; set; }
    public Guid SessionUnitId { get; set; }
    public List<string> ConnectionId { get; set; }
    public List<string> DeviceTypes { get; set; }

}