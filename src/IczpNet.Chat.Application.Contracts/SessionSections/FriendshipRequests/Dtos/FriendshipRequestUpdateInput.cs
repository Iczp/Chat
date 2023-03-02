using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;

public class FriendshipRequestUpdateInput : BaseInput
{
    public virtual long OwnerId { get; set; }

    public virtual long DestinationId { get; set; }

    public virtual string Message { get; set; }
}
