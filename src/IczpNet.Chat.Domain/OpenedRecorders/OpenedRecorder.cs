using IczpNet.AbpCommons;
using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OpenedRecorders
{
    public class OpenedRecorder : BaseEntity, IDeviceId
    {
        public virtual long MessageId { get; protected set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; protected set; }

        public virtual Guid SessionUnitId { get; protected set; }

        [ForeignKey(nameof(SessionUnitId))]
        public virtual SessionUnit SessionUnit { get; protected set; }

        public virtual long? OwnerId { get; protected set; }

        /// <summary>
        /// SessionUnit.Owner
        /// </summary>
        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; protected set; }

        public virtual long? DestinationId { get; protected set; }

        /// <summary>
        /// SessionUnit.Destination
        /// </summary>
        [ForeignKey(nameof(DestinationId))]
        public virtual ChatObject Destination { get; protected set; }

        [StringLength(36)]
        public virtual string DeviceId { get; set; }

        protected OpenedRecorder() { }

        public OpenedRecorder(SessionUnit sessionUnit, Message message, string deviceId)
        {
            SessionUnitId = sessionUnit.Id;
            OwnerId = sessionUnit.OwnerId;
            DestinationId = sessionUnit.DestinationId;
            Message = Assert.NotNull(message, $"notnull:{nameof(message)}");
            MessageId = message.Id;
            DeviceId = deviceId;
        }

        internal void SetMessage(Message message, string deviceId)
        {
            if (message.Id <= MessageId)
            {
                return;
            }
            Message = message;
            MessageId = message.Id;
        }

        public override object[] GetKeys()
        {
            return new object[] { SessionUnitId, MessageId };
        }
    }
}
