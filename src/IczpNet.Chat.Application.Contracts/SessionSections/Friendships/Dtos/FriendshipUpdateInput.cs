using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.Friendships.Dtos;

public class FriendshipUpdateInput : BaseInput
{
    public virtual long OwnerId { get; set; }

    public virtual long DestinationId { get; set; }
}
