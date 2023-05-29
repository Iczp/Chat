namespace IczpNet.Chat.Settings;

public static class ChatSettings
{
    public const string GroupName = "Chat";

    /* Add constants for setting names. Example:
     * public const string MySettingName = GroupName + ".MySettingName";
     */

    /// <summary>
    /// 好友验证有效期
    /// </summary>
    public const string SessionRequestExpirationHours = GroupName + "Settings:SessionRequestExpirationHours";

    /// <summary>
    /// 会话最大关注数量
    /// </summary>
    public const string MaxFollowingCount = GroupName + "Settings:MaxFollowingCount";

    /// <summary>
    ///  超过{HOURS}小时的消息不能被撤回,默认 24H
    /// </summary>
    public const string AllowRollbackHours = GroupName + "Settings:AllowRollbackHours";

    /// <summary>
    /// 最大收藏大小
    /// </summary>
    public const string MaxFavoriteSize = GroupName + "Settings:MaxFavoriteSize";

    /// <summary>
    /// 最大收藏数量
    /// </summary>
    public const string MaxFavoriteCount = GroupName + "Settings:MaxFavoriteCount";

    /// <summary>
    /// 使用后台作业发送程序最小会话单元数量
    /// </summary>

    public const string UseBackgroundJobSenderMinSessionUnitCount = GroupName + "Settings:UseBackgroundJobSenderMinSessionUnitCount";

    /// <summary>
    /// 好友的最大数量
    /// </summary>

    public const string MaxSessionUnitCount = GroupName + "Settings:MaxSessionUnitCount";
}
