using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.OfficialSections.OfficialMemberTagUnits;
using IczpNet.Chat.OfficialSections.Officials;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OfficialSections.OfficialMemberTags
{
    public class OfficialMemberTag : BaseEntity<Guid>, IName
    {
        public virtual Guid? OfficialId { get; protected set; }

        [ForeignKey(nameof(OfficialId))]
        public virtual Official Official { get; protected set; }

        [StringLength(20)]
        public virtual string Name { get; protected set; }

        public virtual IList<OfficialMemberTagUnit> MemberList { get; set; }

        protected OfficialMemberTag() { }

        public OfficialMemberTag(Guid id, Guid officialId, string name) : base(id)
        {
            OfficialId = officialId;
            SetName(name);
        }

        public virtual void SetName(string name)
        {
            Name = name;
        }
    }
}
