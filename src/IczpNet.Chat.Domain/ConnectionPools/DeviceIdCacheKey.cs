using IczpNet.Chat.CacheKeys;
using System;

namespace IczpNet.Chat.ConnectionPools;

public class DeviceIdCacheKey : CacheKey<DeviceIdCacheKey>
{
    public string Type { get; set; }

    public string DeviceId { get; set; }

    public override string ToString()
    {
        return $"{Type}-{nameof(DeviceId)}:{DeviceId}";
    }

    protected override int GetKeyHashCode()
    {
        return HashCode.Combine(Type, DeviceId);
    }

    protected override bool EqualsCore(DeviceIdCacheKey other)
    {
        throw new System.NotImplementedException();
    }

    public DeviceIdCacheKey()
    {

    }

    public DeviceIdCacheKey(string deviceId)
    {
        DeviceId = deviceId;
    }
}
