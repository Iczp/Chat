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
    public const string SessionRequestExpirationHours = GroupName + ".SessionRequest.ExpirationHours";

    /// <summary>
    /// 会话最大关注数量
    /// </summary>
    public const string MaxFollowingCount = GroupName + ".MaxFollowingCount";

    /// <summary>
    ///  超过{HOURS}小时的消息不能被撤回,默认 24H
    /// </summary>
    public const string AllowRollbackHours = GroupName + ".AllowRollbackHours";

    public const string MaxFavoriteSize = GroupName + ".MaxFavoriteSize";

    public const string MaxFavoriteCount = GroupName + ".MaxFavoriteCount";

}
