using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Mottos
{
    public class Motto : BaseEntity<Guid>, IChatOwner<long>
    {
        [MaxLength(500)]
        public virtual string Title { get; set; }

        public virtual long OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        [InverseProperty(nameof(ChatObject.Motto))]
        public virtual IList<ChatObject> ChatObjectList { get; set; }
    }
}
