using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.SessionTags;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class SessionUnitSenderInfo: ISessionUnitSenderInfo
    {
        public virtual Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// 会话内名称（如：群内名称）
        /// </summary>
        public virtual string MemberName { get; set; }

        //public virtual Guid SessionId { get; set; }

        //public virtual long OwnerId { get; set; }

        public virtual IChatObject Owner { get; set; }

        public virtual bool IsPublic { get; set; }

        public virtual bool IsStatic { get; set; }

        public virtual bool IsCreator { get; set; }

        public virtual List<SessionTagInfo> TagList { get; set; }
    }
}
