using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;

public class FriendshipRequestGetListInput : BaseGetListInput
{
    public virtual Guid? OwnerId { get; set; }

    public virtual Guid? DestinationId { get; set; }

    public virtual bool? IsHandled { get; set; }

    public virtual bool? IsAgreed { get; set; }

    public virtual DateTime? StartHandlTime { get; set; }

    public virtual DateTime? EndHandlTime { get; set; }

    public virtual DateTime? StartCreationTime { get; set; }

    public virtual DateTime? EndCreationTime { get; set; }
}
