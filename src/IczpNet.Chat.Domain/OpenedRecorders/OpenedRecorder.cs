using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;

namespace IczpNet.Chat.OpenedRecorders
{
    public class OpenedRecorder : BaseRecorder
    {
        protected OpenedRecorder() { }

        public OpenedRecorder(SessionUnit sessionUnit, long messageId, string deviceId) : base(sessionUnit, messageId, deviceId) { }

        public OpenedRecorder(Guid sessionUnitId, long messageId) : base(sessionUnitId, messageId) { }
    }
}
