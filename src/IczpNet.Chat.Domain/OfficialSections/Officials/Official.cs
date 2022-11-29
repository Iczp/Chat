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
        public const ChatObjectTypeEnum ChatObjectTypeValue = ChatObjectTypeEnum.Official;


        public virtual IList<OfficialGroup> OfficialGroupList { get; set; }


        public virtual IList<OfficalExcludedMember> OfficalExcludedMemberList { get; set; }

        [InverseProperty(nameof(OfficialMember.Official))]
        public virtual IList<OfficialMember> MemberList { get; set; }


        protected Official()
        {
            ChatObjectType = ChatObjectTypeValue;
        }
        protected Official(Guid id) : base(id, ChatObjectTypeValue) { }
    }
}
