using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.OfficialSections.Officials;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OfficialSections.OfficialExcludedMembers
{
    public class OfficalExcludedMember : BaseEntity<Guid>, IChatOwner<Guid>
    {

        public virtual Guid OfficialId { get; set; }

        public virtual Guid OwnerId { get; set; }

        [ForeignKey(nameof(OfficialId))]
        public virtual Official Official { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        protected OfficalExcludedMember()
        {

        }

        protected OfficalExcludedMember(Guid id, Guid chatObjectId) : base(id)
        {
            OwnerId = chatObjectId;
        }
    }
}
