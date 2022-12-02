using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionSections.Friendships.Dtos;

public class FriendshipDto : BaseDto<Guid>
{
    public virtual Guid OwnerId { get; set; }

    //public virtual Guid? FriendId { get;  set; }

    //public virtual ChatObjectSimpleDto Owner { get; set; }

    public virtual ChatObjectSimpleDto Friend { get; set; }

    public virtual bool IsPassive { get; set; }
    
}
