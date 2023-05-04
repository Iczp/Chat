using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.SessionUnits;

namespace IczpNet.Chat.ReadedRecorders
{
    public class ReadedRecorder : BaseRecorder
    {

        protected ReadedRecorder() { }

        public ReadedRecorder(SessionUnit sessionUnit, long messageId, string deviceId) : base(sessionUnit, messageId, deviceId) { }

    }
}
