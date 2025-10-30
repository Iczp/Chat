using System;

namespace IczpNet.Chat.ConnectionPools;

public class IndexCacheKey
{
    public long? ChatObjectId { get; set; }

    public Guid? UserId { get; set; }


    public IndexCacheKey(long? chatObjectId)
    {
        ChatObjectId = chatObjectId;

    }

    public IndexCacheKey(Guid? userId)
    {
        UserId = userId;
    }

    public override string ToString()
    {
        if (UserId.HasValue)
        {
            return $"{nameof(IndexCacheKey)}:{UserId}";
        }
        else if (ChatObjectId.HasValue)
        {
            return $"{nameof(IndexCacheKey)}:{ChatObjectId}";
        }
        return $"{nameof(IndexCacheKey)}-{UserId}-{ChatObjectId}";
    }
}