using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Friends;

public class FriendStatusGetListInput : GetListInput
{
    public Guid? UserId { get; set; }
}
