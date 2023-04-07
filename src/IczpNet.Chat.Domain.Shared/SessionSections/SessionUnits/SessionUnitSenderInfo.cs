using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.SessionTags;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class SessionUnitSenderInfo
    {
        public virtual Guid Id { get; set; }

        public virtual string Rename { get; set; }

        public virtual string MemberName { get; set; }

        //public virtual Guid SessionId { get; set; }

        //public virtual long OwnerId { get; set; }

        public virtual ChatObjectInfo Owner { get; set; }

        public virtual bool IsPublic { get; set; }

        public virtual bool IsStatic { get; set; }

        public virtual List<SessionTagInfo> TagList { get; set; }
    }
}
