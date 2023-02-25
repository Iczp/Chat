using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.OfficialSections.OfficialExcludedMembers;
using IczpNet.Chat.OfficialSections.OfficialGroups;
using IczpNet.Chat.OfficialSections.OfficialMembers;
using IczpNet.Chat.OfficialSections.OfficialMemberTags;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.OfficialSections.Officials
{
    public class Official : ChatObject
    {
        public const ChatObjectTypeEnums ChatObjectTypeValue = ChatObjectTypeEnums.Official;

        public virtual OfficialTypes Type { get; set; }

        /// <summary>
        /// 会话Id
        /// </summary>
        public virtual Guid? SessionId { get; protected set; }

        /// <summary>
        /// 会话
        /// </summary>
        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; set; }

        [InverseProperty(nameof(OfficialGroup.Official))]
        public virtual IList<OfficialGroup> OfficialGroupList { get; set; } = new List<OfficialGroup>();

        public virtual IList<OfficalExcludedMember> OfficalExcludedMemberList { get; set; } = new List<OfficalExcludedMember>();

        [InverseProperty(nameof(OfficialMember.Official))]
        public virtual IList<OfficialMember> OfficialMemberList { get; set; } = new List<OfficialMember>();

        [InverseProperty(nameof(OfficialMemberTag.Official))]
        public virtual IList<OfficialMemberTag> TagList { get; set; } = new List<OfficialMemberTag>();


        protected Official()
        {
            ObjectType = ChatObjectTypeValue;
        }

        public Official(Guid id, string name, Guid? parnetId) : base(id, name, ChatObjectTypeValue, parnetId) { }

        public int GetMemberCount()
        {
            return OfficialMemberList.Count;
        }

        public int GetGroupCount()
        {
            return OfficialGroupList.Count;
        }
    }
}
