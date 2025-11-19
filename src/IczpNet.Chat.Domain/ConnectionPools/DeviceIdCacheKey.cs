namespace IczpNet.Chat.ConnectionPools;

public class DeviceIdCacheKey
{
    public string Type { get; set; }

    public string DeviceId { get; set; }

    public override string ToString()
    {
        return $"{Type}-{nameof(DeviceId)}:{DeviceId}";
    }

    public DeviceIdCacheKey()
    {

    }

    public DeviceIdCacheKey(string deviceId)
    {
        DeviceId = deviceId;
    }
}
