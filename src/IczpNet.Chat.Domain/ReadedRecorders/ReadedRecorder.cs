using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.SessionUnits;
using System;

namespace IczpNet.Chat.ReadedRecorders
{
    public class ReadedRecorder : BaseRecorder
    {

        protected ReadedRecorder() { }

        public ReadedRecorder(SessionUnit sessionUnit, long messageId, string deviceId) : base(sessionUnit, messageId, deviceId) { }

        public ReadedRecorder(Guid sessionUnitId, long messageId) : base(sessionUnitId, messageId) { }
    }
}
