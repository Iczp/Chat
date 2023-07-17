using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Follows
{
    public class Follow : BaseEntity
    {
        /// <summary>
        /// Owner SessionUnitId
        /// </summary>
        public virtual Guid SessionUnitId { get; protected set; }

        /// <summary>
        /// SessionUnit
        /// </summary>
        [ForeignKey(nameof(SessionUnitId))]
        public virtual SessionUnit Owner { get; protected set; }

        /// <summary>
        /// Destination SessionUnitId
        /// </summary>
        public virtual Guid DestinationId { get; protected set; }

        //[ForeignKey(nameof(DestinationId))]
        //public virtual SessionUnit Destination { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { SessionUnitId, DestinationId };
        }

        protected Follow() { }

        public Follow(SessionUnit owner, Guid destinationId)
        {
            Owner = owner; 
            DestinationId = destinationId;
        }
    }
}
