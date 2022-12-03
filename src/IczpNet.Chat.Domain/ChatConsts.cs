using IczpNet.Chat.Enums;
using System.Collections.Generic;

namespace IczpNet.Chat;

public static class ChatConsts
{
    public static List<ChatObjectTypeEnum> AllowJoinRoomMemberObjectTypes { get; set; } = new List<ChatObjectTypeEnum>() {
        ChatObjectTypeEnum.Personal,
        ChatObjectTypeEnum.ShopKeeper,
        ChatObjectTypeEnum.ShopWaiter,
        ChatObjectTypeEnum.Customer,
    };
}
