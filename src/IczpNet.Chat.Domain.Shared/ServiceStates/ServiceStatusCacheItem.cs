using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Data;

namespace IczpNet.Chat.ServiceStates;

public class ServiceStatusCacheItem : IHasExtraProperties
{
    public ServiceStatus? Status { get; set; }

    public DateTime? ActiveTime { get; set; }

    public long? ChatObjectId { get; set; }

    public string DeviceId { get; set; }

    public ExtraPropertyDictionary ExtraProperties { get; set; }

    //public Dictionary<string, string> ExtraProperties { get; set; }

    public ServiceStatusCacheItem() { }

    public ServiceStatusCacheItem(long chatObjectId, string deviceId, ServiceStatus status, ExtraPropertyDictionary extra = null)
    {
        ChatObjectId = chatObjectId;
        Status = status;
        DeviceId = deviceId;
        ActiveTime = DateTime.Now;
        ExtraProperties = extra;
    }

    public override string ToString()
    {
        return $"ChatObjectId:{ChatObjectId},Status:{Status},ActiveTime:{ActiveTime}";
    }
}
