namespace IczpNet.Chat.ServiceStates;

public class ServiceStatusCacheKey
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
}
