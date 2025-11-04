namespace IczpNet.Chat.ScanLogins;

public class ScanLoginCommandConsts
{

    /// <summary>
    /// 欢迎 default: welcome
    /// </summary>
    public static string Welcome { get; set; } = nameof(Welcome).ToLower();

    /// <summary>
    /// 扫码成功 default: scanned
    /// </summary>
    public static string Scanned { get; set; } = nameof(Scanned).ToLower();

    /// <summary>
    /// 拒绝授权 default: forwarded@message
    /// </summary>
    public static string Rejected { get; set; } = nameof(Rejected).ToLower();

    /// <summary>
    /// 同意授权 default: updated-badge@message
    /// </summary>
    public static string Granted { get; set; } = nameof(Granted).ToLower();

}
