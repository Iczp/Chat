using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 会话单元处置类型
/// </summary>
[Description("会话单元处置类型")]
public enum SessionUnitDisposalType : byte
{
    /// <summary>
    /// 移出会话（保留历史）：被踢、群解散、主动退群保留记录
    /// </summary>
    [Description("移出会话（保留历史）")]
    Remove = 1,

    /// <summary>
    /// 彻底删除（软删除，清理消息）：删除好友、退群并删除记录、主动删除会话
    /// </summary>
    [Description("彻底删除会话")]
    Delete = 2,

    /// <summary>
    /// 仅隐藏会话（不删消息，新消息到来时自动浮现）
    /// </summary>
    [Description("隐藏会话")]
    Hide = 3,

    /// <summary>
    /// 清空消息记录（保留会话关系）
    /// </summary>
    [Description("清空消息记录")]
    ClearMessage = 4
}
