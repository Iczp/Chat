namespace IczpNet.Chat.Enums;

/// <summary>
/// URL 安全风控等级
/// </summary>
public enum UrlSecurityLevel
{
    /// <summary>
    /// 官方 / 受信任白名单
    /// </summary>
    Safe = 0,

    /// <summary>
    /// 未知第三方外链（灰名单，需中转警示）
    /// </summary>
    Warning = 1,

    /// <summary>
    /// 违规涉诈 / 恶意软件（黑名单，强力阻断）
    /// </summary>
    Blocked = 2
}
