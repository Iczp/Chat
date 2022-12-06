using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.OfficialSections.OfficialExcludedMembers;
using IczpNet.Chat.OfficialSections.OfficialGroups;
using IczpNet.Chat.OfficialSections.OfficialMembers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OfficialSections.Officials
{
    public class Official : ChatObject
    {
        public const ChatObjectTypes ChatObjectTypeValue = ChatObjectTypes.Official;

        public virtual OfficialTypes Type { get; set; }

        public virtual IList<OfficialGroup> OfficialGroupList { get; set; } = new List<OfficialGroup>();

        public virtual IList<OfficalExcludedMember> OfficalExcludedMemberList { get; set; } = new List<OfficalExcludedMember>();

        [InverseProperty(nameof(OfficialMember.Official))]
        public virtual IList<OfficialMember> MemberList { get; set; } = new List<OfficialMember>();


        protected Official()
        {
            ObjectType = ChatObjectTypeValue;
        }

        protected Official(Guid id) : base(id, ChatObjectTypeValue) { }

        public int GetMemberCount()
        {
            return MemberList.Count;
        }

        public int GetGroupCount()
        {
            return OfficialGroupList.Count;
        }
    }
}
