using IczpNet.Chat.Enums;
using System.Collections.Generic;
using System.IO;

namespace IczpNet.Chat;

public static class ChatConsts
{
    /// <summary>
    /// 允许加入群聊的类型
    /// </summary>
    public static List<ChatObjectTypeEnums> AllowJoinRoomObjectTypes { get; set; } = [
        ChatObjectTypeEnums.Personal,
        ChatObjectTypeEnums.ShopKeeper,
        ChatObjectTypeEnums.ShopWaiter,
        ChatObjectTypeEnums.Customer,
        ChatObjectTypeEnums.Robot,
    ];

    public static string GroupAssistant { get; set; } = nameof(GroupAssistant);

    public static string PrivateAssistant { get; set; } = nameof(PrivateAssistant);

    public const int DriveIdLength = 128;
}
