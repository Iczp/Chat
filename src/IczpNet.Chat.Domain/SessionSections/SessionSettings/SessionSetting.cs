using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionSettings
{
    public class SessionSetting : BaseEntity<Guid>, IOwner<Guid>
    {
        public virtual Guid OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        public virtual Guid? DestinationId { get; set; }

        [ForeignKey(nameof(DestinationId))]
        public virtual ChatObject Destination { get; set; }

        
    }
}
