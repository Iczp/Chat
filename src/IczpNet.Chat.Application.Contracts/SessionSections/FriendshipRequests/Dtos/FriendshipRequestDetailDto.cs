using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;

public class FriendshipRequestDetailDto : FriendshipRequestDto
{
    public virtual string HandlMessage { get; set; }

    public virtual DateTime? HandlTime { get; set; }

    public virtual List<Guid> FriendshipIdList { get; set; }
}
