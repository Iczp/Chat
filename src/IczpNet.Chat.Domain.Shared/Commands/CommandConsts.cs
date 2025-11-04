namespace IczpNet.Chat.Commands;

public class CommandConsts
{

    public const string Chat = nameof(Chat);

    public const string Rollback = nameof(Rollback);

    public const string SessionRequest = nameof(SessionRequest);

    public const string IncrementCompleted = nameof(IncrementCompleted);

    public const string SettingChanged = nameof(SettingChanged);


    /// <summary>
    /// 欢迎 default：welcome
    /// </summary>
    public static string Welcome { get; set; } = nameof(Welcome).ToLower();

    /// <summary>
    /// 新消息 default: created@message
    /// </summary>
    public static string MessageCreated { get; set; } = "created@message";

    /// <summary>
    /// 消息变更 default: updated@message
    /// </summary>
    public static string MessageUpdated { get; set; } = "updated@message";

    /// <summary>
    /// 转发消息 default: forwarded@message
    /// </summary>
    public static string MessageForwarded { get; set; } = "forwarded@message";

    /// <summary>
    /// 更新消息角标 default: updated-badge@message
    /// </summary>
    public static string MessageUpdatedBadge { get; set; } = "updated-badge@message";

    /// <summary>
    /// 删除消息 default: deleted@message
    /// </summary>
    public static string MessageDeleted { get; set; } = "deleted@message";

    /// <summary>
    /// 我上线了 default: online@me
    /// </summary>
    public static string MeOnline { get; set; } = "online@me";

    /// <summary>
    /// 我下线了 default: offline@me
    /// </summary>
    public static string MeOffline { get; set; } = "offline@me";

    /// <summary>
    /// 好友上线了 default: online@friend
    /// </summary>
    public static string FriendOnline { get; set; } = "online@friend";

    /// <summary>
    /// 好友下线了 default: offline@friend
    /// </summary>
    public static string FriendOffline { get; set; } = "offline@friend";

    /// <summary>
    /// 踢出 default: kicked
    /// </summary>
    public static string Kicked { get; set; } = nameof(Kicked).ToLower();

}
