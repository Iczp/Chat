namespace IczpNet.Chat.AppVersions;

public class GetLatestInput
{
    /// <summary>
    /// AppId
    /// </summary>
    public string AppId { get; set; }
    /// <summary>
    /// ios android window mac linux
    /// </summary>
    public string Platform { get; set; }

    /// <summary>
    /// UUID
    /// </summary>
    public string DeviceId { get; set; }

    /// <summary>
    /// 整数
    /// </summary>
    public long VersionCode { get; set; }
}
