using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Messages;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.OpenedRecorders
{
    public class OpenedRecorder : BaseSessionEntity, IDeviceId
    {
        [StringLength(36)]
        public virtual string DeviceId { get; set; }

        public virtual long? MessageAutoId { get; set; }

        public virtual Guid? MessageId { get; set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; set; }
    }
}
