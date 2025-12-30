using System;

namespace IczpNet.Chat.SessionUnits;

public  class FriendModel
{
    public Guid Id { get; set; }

    public long OwnerId { get; set; }

    public Guid SessionId { get; set; }

    public double Sorting { get; set; }

    public double Ticks { get; set; }
}
