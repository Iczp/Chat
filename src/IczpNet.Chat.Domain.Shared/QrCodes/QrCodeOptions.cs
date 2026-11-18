using System.Collections.Generic;

namespace IczpNet.Chat.QrCodes;

public class QrCodeOptions
{
    /// <summary>
    /// 默认生成的二维码内容模板，必须包含 {code}
    /// </summary>
    public string ContentTemplate { get; set; } = "gotoim://qr/{code}";

    /// <summary>
    /// 识别器兼容的二维码内容模板列表
    /// </summary>
    public List<string> CompatibleTemplates { get; set; } =
    [
        "gotoim://qr/{code}",
        "gotoim-dev://qr/{code}",
        "gotoim://card/{code}",
        "gotoim-dev://card/{code}",
        "https://gotoim.com/q/{code}",
        "http://gotoim.com/q/{code}"
    ];

    /// <summary>
    /// 扫码登录模板，用于兼容统一扫码入口识别
    /// </summary>
    public string ScanLoginTemplate { get; set; } = "gotoim://scan-login?code={code}";

    /// <summary>
    /// 扫码登录兼容模板列表
    /// </summary>
    public List<string> ScanLoginCompatibleTemplates { get; set; } =
    [
        "gotoim://scan-login?code={code}",
        "gotoim-dev://scan-login?code={code}",
        "gotoim://scan-login/{code}",
        "gotoim-dev://scan-login/{code}"
    ];

    /// <summary>
    /// 个人/普通名片选择临时码时的默认有效天数
    /// </summary>
    public int DefaultTemporaryExpireDays { get; set; } = 7;

    /// <summary>
    /// 群聊邀请二维码默认有效天数
    /// </summary>
    public int DefaultGroupExpireDays { get; set; } = 7;

    /// <summary>
    /// 允许设置的最大自定义有效天数
    /// </summary>
    public int MaxExpireDays { get; set; } = 365;

    /// <summary>
    /// 缓存默认保留小时数
    /// </summary>
    public int CacheExpirationHours { get; set; } = 24;

    /// <summary>
    /// 临时二维码可复用的最小剩余有效小时数（低于此阈值或不足总有效期 30% 时自动签发新码）
    /// </summary>
    public int ReuseMinRemainingHours { get; set; } = 24;

    /// <summary>
    /// 已停用或已过期二维码的物理清理保留天数
    /// </summary>
    public int CleanupRetentionDays { get; set; } = 30;
}
