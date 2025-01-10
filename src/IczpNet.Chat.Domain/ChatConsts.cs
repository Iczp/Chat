using IczpNet.Chat.Enums;
using System.Collections.Generic;

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

    public const int TextContentMaxLength = 5000;

    public const int VideoUrlMaxLength = 1000;

    public const int SnapshotUrlMaxLength = 1000;

    public const int SnapshotThumbnailUrlMaxLength = 1000;

    public const int GifUrlMaxLength = 1000;
    


}
