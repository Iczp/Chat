using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.SessionUnits;

namespace IczpNet.Chat.Favorites
{
    public class Favorite : BaseRecorder
    {
        protected Favorite() { }

        public Favorite(SessionUnit sessionUnit, long messageId, string deviceId) : base(sessionUnit, messageId, deviceId) { }
    }
}
