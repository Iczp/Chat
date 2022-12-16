using System;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionDto
    {
        public virtual Guid Id { get; set; }
        public virtual string SessionKey { get; set; }

        public virtual int MemberCount { get; set; }
    }
}
