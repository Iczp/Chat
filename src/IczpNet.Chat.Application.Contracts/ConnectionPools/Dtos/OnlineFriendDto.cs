using System;
using System.Collections.Generic;

namespace IczpNet.Chat.ConnectionPools.Dtos;

public class OnlineFriendDto //: OnlineFriendInfo
{
    public virtual long OwnerId { get; set; }
    public virtual long DestinationId { get; set; }
    public virtual Guid SessionId { get; set; }
    public virtual Guid SessionUnitId { get; set; }
    //public virtual List<string> ConnectionId { get; set; }
    public virtual List<string> DeviceTypes { get; set; }
}
