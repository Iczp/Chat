using System;

namespace IczpNet.Chat.DeletedRecorders;

public class DeletedRecorderCacheKey
{
    public Guid SessionUnitId { get; set; }
    public override string ToString()
    {
        return $"${nameof(DeletedRecorderCacheKey)}-{nameof(SessionUnitId)}:{SessionUnitId}";
    }
    public DeletedRecorderCacheKey()
    {

    }
    public DeletedRecorderCacheKey(Guid sessionUnitId)
    {
        SessionUnitId = sessionUnitId;
    }
}
