using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.SessionUnits;
using System;

namespace IczpNet.Chat.DeletedRecorders;

public class DeletedRecorder : BaseRecorder
{
    public virtual long? ChangeSeq { get; protected set; }

    protected DeletedRecorder() { }

    public DeletedRecorder(SessionUnit sessionUnit, long messageId, string deviceId) : base(sessionUnit, messageId, deviceId) { }

    public DeletedRecorder(SessionUnit sessionUnit, long messageId, string deviceId, long changeSeq)
        : this(sessionUnit, messageId, deviceId)
    {
        ChangeSeq = changeSeq;
    }

    public DeletedRecorder(Guid sessionUnitId, long messageId) : base(sessionUnitId, messageId) { }
}
