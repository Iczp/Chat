using IczpNet.Chat.Enums;
using System.Collections.Generic;

namespace IczpNet.Chat;

public static class ChatConsts
{
    public static List<ChatObjectTypes> AllowJoinRoomMemberObjectTypes { get; set; } = new List<ChatObjectTypes>() {
        ChatObjectTypes.Personal,
        ChatObjectTypes.ShopKeeper,
        ChatObjectTypes.ShopWaiter,
        ChatObjectTypes.Customer,
    };
}
