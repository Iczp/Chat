using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.Friendships.Dtos;

public class FriendshipUpdateInput : BaseInput
{
    public virtual Guid OwnerId { get; set; }

    public virtual Guid DestinationId { get; set; }
}
