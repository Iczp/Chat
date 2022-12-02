using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;

public class FriendshipRequestUpdateInput : BaseInput
{
    public virtual Guid OwnerId { get; set; }

    public virtual Guid DestinationId { get; set; }

    public virtual string Message { get; set; }
}
