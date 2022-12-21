using IczpNet.Chat.ChatObjects.Dtos;
using System;

namespace IczpNet.Chat.SessionSections.Sessions.Dtos
{
    public class SessionDto
    {
        public virtual Guid Id { get; set; }

        public virtual string SessionKey { get; set; }

        public virtual ChatObjectDto Owner { get; set; }

        public virtual int MemberCount { get; set; }
    }
}
