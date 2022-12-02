using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.ChatObjects.Dtos;
using System;

namespace IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;

public class FriendshipRequestDto : BaseDto<Guid>
{
    public virtual Guid OwnerId { get; set; }

    //public virtual Guid? DestinationId { get; set; }

    //public virtual ChatObjectSimpleDto Owner { get; set; }

    public virtual ChatObjectSimpleDto Destination { get; set; }

    public virtual bool IsHandled { get; set; }

    public virtual bool? IsAgreed { get; set; }

    public virtual string Message { get; set; }

}
