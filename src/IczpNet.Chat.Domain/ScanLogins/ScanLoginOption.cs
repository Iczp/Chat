using System.Collections.Generic;

namespace IczpNet.Chat.ScanLogins;

public class ScanLoginOption
{

    /// <summary>
    /// 变量名称：{code}，默认值：gotoim://scan-login?code={code}
    /// </summary>
    public string ScanTextTemplate { get; set; } = "gotoim://scan-login?code={code}";

    /// <summary>
    /// 
    /// </summary>
    public string ParamKey { get; set; } = "code";

    /// <summary>
    /// 过期时间（秒） 默认：90
    /// </summary>
    public int ExpiredSeconds { get; set; } = 90;

    /// <summary>
    /// 
    /// </summary>
    public string DistributedCacheKey { get; set; } = "ScanLoginDistributedCacheKey";

    /// <summary>
    /// 
    /// </summary>
    public List<string> AllowedDeviceTypes { get; set; } = ["phone", "tablet", "pad"];

    /// <summary>
    /// 
    /// </summary>
    public bool IsRequiredDeviceType { get; set; } = false;
}
