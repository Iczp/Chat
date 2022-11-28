using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.OfficialSections.Officials;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OfficialSections.OfficialExcludedMembers
{
    public class OfficalExcludedMember : BaseEntity<Guid>
    {

        public virtual Guid OfficialId { get; set; }

        public virtual Guid ChatObjectId { get; set; }

        [ForeignKey(nameof(OfficialId))]
        public virtual Official Official { get; set; }

        [ForeignKey(nameof(ChatObjectId))]
        public virtual ChatObject ChatObject { get; set; }

        protected OfficalExcludedMember()
        {

        }

        protected OfficalExcludedMember(Guid id, Guid chatObjectId) : base(id)
        {
            ChatObjectId = chatObjectId;
        }
    }
}
