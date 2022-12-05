using System;

namespace IczpNet.Chat.SessionSections.OpenedRecordes.Dtos
{
    public class OpenedRecorderInput
    {
        public virtual Guid OwnerId { get; set; }

        public virtual Guid DestinationId { get; set; }

        public virtual string DeviceId { get; set; }

        public virtual Guid MessageId { get; set; }
    }
}
