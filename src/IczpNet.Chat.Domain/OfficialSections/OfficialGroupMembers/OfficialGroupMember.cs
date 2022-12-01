using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.OfficialSections.OfficialGroups;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OfficialSections.OfficialGroupMembers
{
    public class OfficialGroupMember : BaseEntity<Guid>, IOwner<Guid>
    {

        public virtual Guid OwnerId { get; set; }

        public virtual Guid OfficialGroupId { get; set; }

        [ForeignKey(nameof(OfficialGroupId))]
        public virtual OfficialGroup OfficialGroup { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        protected OfficialGroupMember()
        {

        }

        protected OfficialGroupMember(Guid id, Guid chatObjectId) : base(id)
        {
            OwnerId = chatObjectId;
        }
    }
}
