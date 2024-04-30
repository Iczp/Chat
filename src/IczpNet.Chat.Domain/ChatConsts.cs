using IczpNet.Chat.Enums;
using System.Collections.Generic;
using System.IO;

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

    public static string GroupAssistant { get; set; } = nameof(GroupAssistant);

    public static string PrivateAssistant { get; set; } = nameof(PrivateAssistant);

    public const int DriveIdLength = 128;
}
