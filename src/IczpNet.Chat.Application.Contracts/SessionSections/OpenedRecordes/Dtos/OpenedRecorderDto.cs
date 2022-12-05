using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.OpenedRecordes.Dtos
{
    public class OpenedRecorderDto : EntityDto<Guid>
    {
        public virtual Guid OwnerId { get; set; }

        public virtual Guid? DestinationId { get; set; }

        public virtual string DeviceId { get; set; }

        public virtual long? MessageAutoId { get; set; }

        public virtual Guid MessageId { get; set; }
    }
}
