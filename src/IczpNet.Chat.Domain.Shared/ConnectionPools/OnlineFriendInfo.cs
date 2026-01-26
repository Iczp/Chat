using System;

namespace IczpNet.Chat.ConnectionPools;

public class OnlineFriendInfo
{
    public long OwnerId { get; set; }
    public long DestinationId { get; set; }
    public Guid SessionId { get; set; }
    public Guid SessionUnitId { get; set; }
    //public string ConnectionId { get; set; }

}