using System.ComponentModel;

namespace IczpNet.Chat.Settings;

public static class ChatSettings
{
    public const string GroupName = nameof(ChatSettings);

    /* Add constants for setting names. Example:
     * public const string MySettingName = GroupName + ".MySettingName";
     */

    /// <summary>
    /// 消息发送器总开关
    /// </summary>
    [DefaultValue(true)]
    public const string IsMessageSenderEnabled = $"{GroupName}:{nameof(IsMessageSenderEnabled)}";

    /// <summary>
    /// 好友验证有效期
    /// </summary>
    [DefaultValue(72)] 
    public const string SessionRequestExpirationHours = $"{GroupName}:{nameof(SessionRequestExpirationHours)}";

    /// <summary>
    /// 会话最大关注数量
    /// </summary>
    [DefaultValue(10)] 
    public const string MaxFollowingCount = $"{GroupName}:{nameof(MaxFollowingCount)}";

    /// <summary>
    ///  超过{HOURS}小时的消息不能被撤回,默认 24H
    /// </summary>
    [DefaultValue(24)] 
    public const string AllowRollbackHours = $"{GroupName}:{nameof(AllowRollbackHours)}";

    /// <summary>
    /// 最大收藏大小
    /// </summary>
    [DefaultValue(long.MaxValue)] 
    public const string MaxFavoriteSize = $"{GroupName}:{nameof(MaxFavoriteSize)}";

    /// <summary>
    /// 最大收藏数量
    /// </summary>
    [DefaultValue(long.MaxValue)] 
    public const string MaxFavoriteCount = $"{GroupName}:{nameof(MaxFavoriteCount)}";

    /// <summary>
    /// 使用后台作业发送程序最小会话单元数量
    /// </summary>

    [DefaultValue(500)] 
    public const string UseBackgroundJobSenderMinSessionUnitCount = $"{GroupName}:{nameof(UseBackgroundJobSenderMinSessionUnitCount)}";

    /// <summary>
    /// 好友的最大数量
    /// </summary>

    [DefaultValue(5000)] 
    public const string MaxSessionUnitCount = $"{GroupName}:{nameof(MaxSessionUnitCount)}";

    [DefaultValue(128)]
    public const string PortraitThumbnailSize = $"{GroupName}:{nameof(PortraitThumbnailSize)}";

    [DefaultValue(540)]
    public const string PortraitBigSize = $"{GroupName}:{nameof(PortraitBigSize)}";
}
