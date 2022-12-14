using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.OfficialSections.OfficialMemberTagUnits;
using IczpNet.Chat.OfficialSections.Officials;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OfficialSections.OfficialMembers
{
    public class OfficialMember : BaseEntity<Guid>, IChatOwner<Guid>
    {
        public virtual Guid OfficialId { get; set; }

        [ForeignKey(nameof(OfficialId))]
        public virtual Official Official { get; set; }

        public virtual Guid OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        public virtual IList<OfficialMemberTagUnit> TagList { get; set; }
    }
}
