using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 设置加群、加好友、加聊天广场验证方式
/// </summary>
[Description("验证方式")]
public enum VerificationMethods
{
    /// <summary>
    /// 不需要验证
    /// </summary>
    [Description("不需要验证")]
    None = 0,
    /// <summary>
    /// 需要验证
    /// </summary>
    [Description("需要验证")]
    Required = 1,

    /// <summary>
    /// 拒绝验证
    /// </summary>
    [Description("拒绝验证")]
    Rejected = 2,
}
