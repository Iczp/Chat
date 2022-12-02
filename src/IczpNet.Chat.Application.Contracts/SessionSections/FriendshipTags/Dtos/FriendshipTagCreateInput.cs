using System;

namespace IczpNet.Chat.FriendshipTagSections.FriendshipTags.Dtos;

public class FriendshipTagCreateInput : FriendshipTagUpdateInput
{
    public virtual Guid OwnerId { get; set; }
}
