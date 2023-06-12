using IczpNet.Chat.ChatObjects.Dtos;
using System;

namespace IczpNet.Chat.SessionSections.Sessions.Dtos
{
    public class SessionDto
    {
        public virtual Guid Id { get; set; }

        public virtual string Title { get; set; }

        public virtual string Description { get; set; }

        public virtual string SessionKey { get; set; }

        public virtual long? OwnerId { get; set; }

        //public virtual ChatObjectDto Owner { get; set; }

        //public virtual int MemberCount { get; set; }

        //public virtual int MessageCount { get; set; }

        //public virtual int TagCount { get; set; }

        //public virtual int RoleCount { get; set; }
    }
}
