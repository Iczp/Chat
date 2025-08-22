using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.SessionUnits;
using System;

namespace IczpNet.Chat.DeletedRecorders;

public class DeletedRecorder : BaseRecorder
{
    protected DeletedRecorder() { }

    public DeletedRecorder(SessionUnit sessionUnit, long messageId, string deviceId) : base(sessionUnit, messageId, deviceId) { }

    public DeletedRecorder(Guid sessionUnitId, long messageId) : base(sessionUnitId, messageId) { }
}
