using System;

namespace IczpNet.Chat.ConnectionPools;

public class IndexCacheKey: IIndexCacheKey
{
    public long? ChatObjectId { get; set; }

    public Guid? UserId { get; set; }

    public IndexCacheValueType? ValueType { get; set; }

    public IndexCacheKey(long chatObjectId, IndexCacheValueType valueType)
    {
        ChatObjectId = chatObjectId;
        ValueType = valueType;
    }

    public IndexCacheKey(Guid userId, IndexCacheValueType valueType)
    {
        UserId = userId;
        ValueType = valueType;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName">ChatObjectId | UserId</param>
    /// <param name="value"></param>
    /// <param name="valueType"></param>
    public IndexCacheKey(string propertyName , dynamic value, IndexCacheValueType valueType)
    {
        if (propertyName == nameof(ChatObjectId) && value is long chatObjectId)
        {
            ChatObjectId = chatObjectId;
        }
        else if (propertyName == nameof(UserId) && value is Guid userId)
        {
            UserId = userId;
        }
        ValueType = valueType;
    }

    public override string ToString()
    {
        if (UserId.HasValue)
        {
            return $"{nameof(IndexCacheKey)}-{ValueType}-{nameof(UserId)}:{UserId}";
        }
        else if (ChatObjectId.HasValue)
        {
            return $"{nameof(IndexCacheKey)}-{ValueType}-{nameof(ChatObjectId)}:{ChatObjectId}";
        }
        return $"{nameof(IndexCacheKey)}-{ValueType}-UC:{UserId}-{ChatObjectId}";
    }
}