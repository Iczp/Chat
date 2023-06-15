using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.BaseEntities
{
    public abstract class BaseSessionEntity<TKey> : BaseEntity<Guid>, IChatOwner<long>
    {
        public virtual long OwnerId { get; protected set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; protected set; }

        public virtual long? DestinationId { get; protected set; }

        [ForeignKey(nameof(DestinationId))]
        public virtual ChatObject Destination { get; protected set; }

        protected BaseSessionEntity() { }
        protected BaseSessionEntity(Guid id) : base(id) { }
    }
}
