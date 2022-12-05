using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.BaseEntitys
{
    public abstract class BaseSessionEntity : BaseEntity<Guid>, IChatOwner<Guid>
    {
        public virtual Guid OwnerId { get; protected set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; protected set; }

        public virtual Guid? DestinationId { get; protected set; }

        [ForeignKey(nameof(DestinationId))]
        public virtual ChatObject Destination { get; protected set; }
    }
}
