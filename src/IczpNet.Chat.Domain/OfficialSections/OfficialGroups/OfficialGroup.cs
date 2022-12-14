using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.Enums;
using IczpNet.Chat.OfficialSections.OfficialGroupMembers;
using IczpNet.Chat.OfficialSections.Officials;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OfficialSections.OfficialGroups
{
    public class OfficialGroup : BaseEntity<Guid>, IName, IIsPublic //ChatObject
    {
        //public const ChatObjectTypes ChatObjectTypeValue = ChatObjectTypes.OfficialGroup;

        public virtual Guid OfficialId { get; protected set; }

        [ForeignKey(nameof(OfficialId))]
        public virtual Official Official { get; protected set; }

        [StringLength(50)]
        public virtual string Name { get; protected set; }

        [StringLength(200)]
        public virtual string Description { get; protected set; }

        public virtual bool IsPublic { get; protected set; }

        /// <summary>
        /// 服务号接收人分组
        /// </summary>
        public virtual IList<OfficialGroupMember> OfficialGroupMemberList { get; protected set; }

        protected OfficialGroup()
        {
            //ObjectType = ChatObjectTypeValue;
        }

        public OfficialGroup(Guid id, Guid officialId, string name, string description, bool isPublic, IList<OfficialGroupMember> officialGroupMemberList)
            : base(id)
            //: base(id, ChatObjectTypeValue)
        {
            OfficialId = officialId;
            SetName(name);
            Description = description;
            IsPublic = isPublic;
            OfficialGroupMemberList = officialGroupMemberList;
        }

        private void SetName(string name)
        {
            Name = name;
        }

        public int GetGroupMemberCount()
        {
            return OfficialGroupMemberList.Count;
        }
    }
}
