namespace IczpNet.Chat.ScanLogins;

public class ScanLoginCommandConsts
{

    /// <summary>
    /// 欢迎 default: Welcome
    /// </summary>
    public static string Welcome { get; set; } = nameof(Welcome).ToLower();

    /// <summary>
    /// 扫码成功 default: Scanned
    /// </summary>
    public static string Scanned { get; set; } = nameof(Scanned).ToLower();

    /// <summary>
    /// 拒绝授权 default: Rejected
    /// </summary>
    public static string Rejected { get; set; } = nameof(Rejected).ToLower();

    /// <summary>
    /// 取消 default: Cancelled
    /// </summary>
    public static string Cancelled { get; set; } = nameof(Cancelled).ToLower();

    /// <summary>
    /// 同意授权 default: Granted
    /// </summary>
    public static string Granted { get; set; } = nameof(Granted).ToLower();

}
