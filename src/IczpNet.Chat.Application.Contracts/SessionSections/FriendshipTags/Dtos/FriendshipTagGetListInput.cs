using IczpNet.Chat.BaseDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace IczpNet.Chat.FriendshipTagSections.FriendshipTags.Dtos;

public class FriendshipTagGetListInput : BaseGetListInput
{
    [DefaultValue(null)]
    public virtual long? OwnerId { get; set; }
}
