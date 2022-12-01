using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.OfficialSections.OfficialGroups;
using IczpNet.Chat.OfficialSections.Officials;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OfficialSections.OfficialMembers
{
    public class OfficialMember : BaseEntity<Guid>, IOwner<Guid>
    {
        public virtual Guid OfficialId { get; set; }

        [ForeignKey(nameof(OfficialId))]
        public virtual Official Official { get; set; }


        public virtual Guid OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }
    }
}
