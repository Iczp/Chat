using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.OpenedRecorders.Dtos
{
    public class OpenedRecorderInput
    {
        //public virtual long OwnerId { get; set; }

        //public virtual long DestinationId { get; set; }

        [Required]
        public virtual Guid SessionUnitId { get; set; }

        [Required]
        public virtual long MessageId { get; set; }

        public virtual string DeviceId { get; set; }

    }
}
