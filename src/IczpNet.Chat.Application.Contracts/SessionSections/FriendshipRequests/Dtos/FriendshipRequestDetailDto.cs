using System;

namespace IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;

public class FriendshipRequestDetailDto : FriendshipRequestDto
{
    public virtual string HandlMessage { get; set; }

    public virtual DateTime? HandlTime { get; set; }

    public virtual Guid? FriendshipId { get; set; }
}
