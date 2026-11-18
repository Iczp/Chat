namespace IczpNet.Chat.Enums;

/// <summary>
/// URL 风险细分类型
/// </summary>
public enum UrlRiskCategory
{
    None = 0,
    OfficialOrWhitelisted = 1,
    UnknownThirdParty = 2,
    Fraud = 3,            // 电信诈骗/杀猪盘/刷单
    Phishing = 4,         // 仿冒钓鱼
    Porn = 5,             // 色情低俗
    IllegalGambling = 6,  // 非法赌博
    Malware = 7,          // 病毒木马/恶意下载
    Other = 99
}
