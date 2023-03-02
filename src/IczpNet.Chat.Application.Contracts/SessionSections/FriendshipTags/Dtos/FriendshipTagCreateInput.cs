using System;

namespace IczpNet.Chat.FriendshipTagSections.FriendshipTags.Dtos;

public class FriendshipTagCreateInput : FriendshipTagUpdateInput
{
    public virtual long OwnerId { get; set; }
}
