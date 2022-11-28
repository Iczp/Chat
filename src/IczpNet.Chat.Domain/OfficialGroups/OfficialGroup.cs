using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.OfficialGroupMembers;
using IczpNet.Chat.Officials;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OfficialGroups
{
    public class OfficialGroup : BaseEntity<Guid>
    {
        public virtual Guid OfficialId { get; set; }

        [ForeignKey(nameof(OfficialId))]
        public virtual Official Official { get; set; }

        /// <summary>
        /// 服务号接收人分组
        /// </summary>
        public virtual IList<OfficialGroupMember> OfficialGroupMemberList { get; set; }

        protected OfficialGroup()
        {

        }

        protected OfficialGroup(Guid id, Guid officialId) : base(id)
        {
            OfficialId = officialId;
        }
    }
}
