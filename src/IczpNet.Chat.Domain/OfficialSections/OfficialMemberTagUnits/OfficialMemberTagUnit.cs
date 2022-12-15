using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.OfficialSections.OfficialMembers;
using IczpNet.Chat.OfficialSections.OfficialMemberTags;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OfficialSections.OfficialMemberTagUnits
{
    public class OfficialMemberTagUnit : BaseEntity
    {
        public virtual Guid TagId { get; set; }

        [ForeignKey(nameof(TagId))]
        public virtual OfficialMemberTag Tag { get; set; }

        public virtual Guid MemberId { get; set; }

        [ForeignKey(nameof(MemberId))]
        public virtual OfficialMember Member { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { TagId, MemberId };
        }
    }
}
