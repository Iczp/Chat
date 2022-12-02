using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.FriendshipTagSections.FriendshipTags.Dtos;

public class FriendshipTagDto : BaseDto<Guid>
{
    public virtual string Name { get; set; }

    public virtual Guid? OwnerId { get; set; }

    public virtual int FriendshipCount { get; set; }
}
