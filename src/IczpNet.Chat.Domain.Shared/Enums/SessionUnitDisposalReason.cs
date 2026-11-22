using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 会话处置原因细分
/// </summary>
[Description("会话处置原因细分")]
public enum SessionUnitDisposalReason : byte
{
    /// <summary>
    /// 好友删除
    /// </summary>
    [Description("好友删除")]
    FriendDeleted = 1,

    /// <summary>
    /// 被管理员移出群聊（被踢）
    /// </summary>
    [Description("被管理员移出群聊")]
    ManagerKicked = 2,

    /// <summary>
    /// 成员主动退出群聊
    /// </summary>
    [Description("成员主动退出群聊")]
    MemberQuit = 3,

    /// <summary>
    /// 群聊已解散
    /// </summary>
    [Description("群聊已解散")]
    RoomDisbanded = 4,

    /// <summary>
    /// 用户主动删除会话
    /// </summary>
    [Description("用户主动删除会话")]
    UserDeletedSession = 5,

    /// <summary>
    /// 用户主动隐藏会话
    /// </summary>
    [Description("用户主动隐藏会话")]
    UserHiddenSession = 6,

    /// <summary>
    /// 用户主动清空消息
    /// </summary>
    [Description("用户主动清空消息")]
    UserClearedMessages = 7,

    /// <summary>
    /// 成员重新加入群聊（重新激活）
    /// </summary>
    [Description("重新加入群聊")]
    MemberRejoined = 8,

    /// <summary>
    /// 重新添加好友（恢复会话）
    /// </summary>
    [Description("重新添加好友")]
    FriendRestored = 9
}
