

using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 红包发放方式（0：随机金额;1:固定金额）
/// </summary>
[Description("红包发放方式")]
public enum GrantModes : int
{
    /// <summary>
    /// 拼人气红包（随机金额）
    /// </summary>
    [Description("随机金额")] 
    RandomAmount = 0,
    /// <summary>
    /// 固定金额
    /// </summary>
    [Description("固定金额")] 
    FixedAmount = 1,
    /// <summary>
    /// 转账
    /// </summary>
    [Description("转账")] 
    TransferAccounts = 2,
}
