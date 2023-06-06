using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.EntryValues;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.ChatObjectEntryValues
{
    [Index(nameof(CreationTime))]
    public class ChatObjectEntryValue : BaseEntity, IChatOwner<long>
    {
        public virtual long OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        public virtual Guid EntryValueId { get; set; }

        [ForeignKey(nameof(EntryValueId))]
        public virtual EntryValue EntryValue { get; set; }

        public virtual long? DestinationId { get; set; }

        [ForeignKey(nameof(DestinationId))]
        public virtual ChatObject Destination { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { OwnerId, EntryValueId };
        }
    }
}
