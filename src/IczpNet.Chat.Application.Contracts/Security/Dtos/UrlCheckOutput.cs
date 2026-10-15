using IczpNet.Chat.Enums;

namespace IczpNet.Chat.Security.Dtos;

/// <summary>
/// URL 安全检测出参
/// </summary>
public class UrlCheckOutput
{
    /// <summary>
    /// 检测的 URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 域名/主机名
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    /// 安全等级 (0=Safe, 1=Warning, 2=Blocked)
    /// </summary>
    public UrlSecurityLevel Level { get; set; }

    /// <summary>
    /// 风险类别
    /// </summary>
    public UrlRiskCategory Category { get; set; }

    /// <summary>
    /// 提示信息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 拦截/警告原因
    /// </summary>
    public string InterceptReason { get; set; }

    /// <summary>
    /// 规则来源
    /// </summary>
    public string Source { get; set; }
}
