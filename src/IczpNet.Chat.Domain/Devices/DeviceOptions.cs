namespace IczpNet.Chat.Devices;

public class DeviceOptions
{


    /// <summary>
    /// default: App-Device-Id
    /// </summary>
    public string RequestDeviceIdKey { get; set; } = "App-Device-Id";

    /// <summary>
    /// default: App-Device-Type
    /// </summary>
    public string RequestDeviceTypeKey { get; set; } = "App-Device-Type";

    /// <summary>
    /// default: App-Id
    /// </summary>
    public string RequestAppIdKey { get; internal set; } = "App-Id";

    /// <summary>
    /// default: App-Version
    /// </summary>
    public string RequestAppVersionKey { get; internal set; } = "App-Version";
}
