using IczpNet.Chat.Enums;
using System.Collections.Generic;

namespace IczpNet.Chat;

public static class ChatConsts
{
    public static List<ChatObjectTypeEnums> AllowJoinRoomObjectTypes { get; set; } = new List<ChatObjectTypeEnums>() {
        ChatObjectTypeEnums.Personal,
        ChatObjectTypeEnums.ShopKeeper,
        ChatObjectTypeEnums.ShopWaiter,
        ChatObjectTypeEnums.Customer,
        ChatObjectTypeEnums.Robot,
    };
}
