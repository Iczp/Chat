using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.BaseEntities
{
    public abstract class BaseRecorder : BaseEntity, IDeviceId, IMessageId, ISessionUnitId
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

        protected BaseRecorder() { }

        public BaseRecorder(Guid sessionUnitId, long messageId)
        {
            SessionUnitId = sessionUnitId;
            MessageId = messageId;
        }

        public BaseRecorder(SessionUnit sessionUnit, long messageId, string deviceId)
        {
            SessionUnitId = sessionUnit.Id;
            OwnerId = sessionUnit.OwnerId;
            DestinationId = sessionUnit.DestinationId;
            MessageId = messageId;
            DeviceId = deviceId;
        }

        public override object[] GetKeys()
        {
            return new object[] { SessionUnitId, MessageId };
        }
    }
}
