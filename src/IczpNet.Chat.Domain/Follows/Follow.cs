using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Follows
{
    public class Follow : BaseEntity
    {
        public virtual Guid OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual SessionUnit Owner { get; set; }

        public virtual Guid DestinationId { get; set; }

        //[ForeignKey(nameof(DestinationId))]
        //public virtual SessionUnit Destination { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { OwnerId, DestinationId };
        }

        protected Follow() { }

        public Follow(SessionUnit owner, Guid destinationId)
        {
            Owner = owner; 
            DestinationId = destinationId;
        }
    }
}
