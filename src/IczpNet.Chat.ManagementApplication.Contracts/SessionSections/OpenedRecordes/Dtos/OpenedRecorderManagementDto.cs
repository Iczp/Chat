using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Management.SessionSections.OpenedRecordes.Dtos
{
    public class OpenedRecorderManagementDto : EntityDto<Guid>
    {
        public virtual long OwnerId { get; set; }

        public virtual long? DestinationId { get; set; }

        public virtual string DeviceId { get; set; }

        public virtual long? MessageAutoId { get; set; }

        public virtual long MessageId { get; set; }
    }
}
