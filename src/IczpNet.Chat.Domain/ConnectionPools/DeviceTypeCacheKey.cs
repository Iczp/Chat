using System;

namespace IczpNet.Chat.ConnectionPools;

public class DeviceTypeCacheKey
{
    public long? ChatObjectId { get; set; }

    public Guid? UserId { get; set; }


    public DeviceTypeCacheKey(long? chatObjectId)
    {
        ChatObjectId = chatObjectId;

    }

    public DeviceTypeCacheKey(Guid? userId)
    {
        UserId = userId;
    }

    public override string ToString()
    {
        if (UserId.HasValue)
        {
            return $"{nameof(DeviceTypeCacheKey)}:{UserId}";
        }
        else if (ChatObjectId.HasValue)
        {
            return $"{nameof(DeviceTypeCacheKey)}:{ChatObjectId}";
        }
        return $"{nameof(DeviceTypeCacheKey)}-{UserId}-{ChatObjectId}";
    }
}