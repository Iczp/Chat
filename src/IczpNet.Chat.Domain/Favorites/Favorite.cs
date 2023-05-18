using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;

namespace IczpNet.Chat.Favorites
{
    public class Favorite : BaseRecorder
    {
        public virtual long Size { get; protected set; }

        public virtual MessageTypes MessageType { get; protected set; }

        protected Favorite() { }

        public Favorite(SessionUnit sessionUnit, Message message, string deviceId) : base(sessionUnit, message.Id, deviceId)
        {
            Size = message.Size;
            MessageType = message.MessageType;
        }

        public Favorite(Guid sessionUnitId, long messageId) : base(sessionUnitId, messageId) { }
    }
}
