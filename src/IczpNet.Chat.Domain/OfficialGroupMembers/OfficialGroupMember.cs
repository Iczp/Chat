using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.OfficialGroups;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OfficialGroupMembers
{
    public class OfficialGroupMember : BaseEntity<Guid>
    {

        public virtual Guid ChatObjectId { get; set; }

        public virtual Guid OfficialGroupId { get; set; }

        [ForeignKey(nameof(OfficialGroupId))]
        public virtual OfficialGroup OfficialGroup { get; set; }

        [ForeignKey(nameof(ChatObjectId))]
        public virtual ChatObject ChatObject { get; set; }

        protected OfficialGroupMember()
        {

        }

        protected OfficialGroupMember(Guid id, Guid chatObjectId) : base(id)
        {
            ChatObjectId = chatObjectId;
        }
    }
}
