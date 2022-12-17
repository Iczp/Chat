using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using System;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionUnitDto
    {
        public virtual Guid SessionId { get; set; }

        public virtual MessageDto Message { get; set; }

        public virtual ChatObjectDto Destination { get; set; }

        public virtual int Badge { get; set; }
    }
}
