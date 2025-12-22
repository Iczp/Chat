using IczpNet.Chat.CacheKeys;
using System;
namespace IczpNet.Chat.ServiceStates;

public class ServiceStatusCacheKey : CacheKey<ServiceStatusCacheKey>
{
    public long? ChatObjectId { get; set; }
    public string DeviceId { get; set; }
    public ServiceStatusCacheKey() { }
    public ServiceStatusCacheKey(long chatObjectId, string deviceId)
    {
        ChatObjectId = chatObjectId;
        DeviceId = deviceId;
    }
    public override string ToString()
    {
        return $"ChatObjectId:{ChatObjectId},DeviceId:{DeviceId}";
    }

    protected override int GetKeyHashCode()
    {
        return HashCode.Combine(ChatObjectId, DeviceId);
    }

    protected override bool EqualsCore(ServiceStatusCacheKey other)
    {
        return ChatObjectId == other.ChatObjectId && DeviceId == other.DeviceId;
    }
}
