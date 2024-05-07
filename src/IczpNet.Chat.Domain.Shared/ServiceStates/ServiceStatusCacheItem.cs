using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.ServiceStates;

public class ServiceStatusCacheItem
{
    public ServiceStatus? Status { get; set; }

    public DateTime? ActiveTime { get; set; }

    public long? ChatObjectId { get; set; }

    public string DeviceId { get; set; }

    public Dictionary<string, string> Extra { get; set; }

    public ServiceStatusCacheItem() { }

    public ServiceStatusCacheItem(long chatObjectId, string deviceId, ServiceStatus status, Dictionary<string, string> extra = null)
    {
        ChatObjectId = chatObjectId;
        Status = status;
        DeviceId = deviceId;
        ActiveTime = DateTime.Now;
        Extra = extra;
    }

    public override string ToString()
    {
        return $"ChatObjectId:{ChatObjectId},Status:{Status},ActiveTime:{ActiveTime}";
    }
}
