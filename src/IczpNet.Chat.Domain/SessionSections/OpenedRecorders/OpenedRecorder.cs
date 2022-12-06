using IczpNet.AbpCommons;
using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.OpenedRecorders
{
    public class OpenedRecorder : BaseSessionEntity, IDeviceId
    {
        [StringLength(36)]
        public virtual string DeviceId { get; set; }

        public virtual long? MessageAutoId { get; protected set; }

        public virtual Guid? MessageId { get; protected set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; protected set; }

        protected OpenedRecorder() { }

        public OpenedRecorder(Guid ownerId, Guid destinationId, Message message, string deviceId)
        {
            OwnerId = ownerId;
            DestinationId = destinationId;
            Message = Assert.NotNull(message, $"notnull:{nameof(message)}");
            MessageAutoId = message.AutoId;
            DeviceId = deviceId;
        }

        internal void SetMessage(Message message, string deviceId)
        {
            if (message.AutoId <= MessageAutoId)
            {
                return;
            }
            Message = message;
            MessageAutoId = message.AutoId;
        }
    }
}
