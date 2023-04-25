using System;

namespace IczpNet.Chat.Management.SessionSections.OpenedRecordes.Dtos
{
    public class OpenedRecorderManagementInput
    {
        public virtual long OwnerId { get; set; }

        public virtual long DestinationId { get; set; }

        public virtual string DeviceId { get; set; }

        public virtual long MessageId { get; set; }
    }
}
