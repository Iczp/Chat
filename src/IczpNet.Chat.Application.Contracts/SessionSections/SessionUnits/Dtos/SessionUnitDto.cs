using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using System;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitDto
    {
        public virtual Guid Id { get; set; }

        public virtual Guid SessionId { get; set; }

        public virtual Guid OwnerId { get; set; }

        public virtual MessageDto LastMessage { get; set; }

        public virtual ChatObjectDto Destination { get; set; }

        public virtual int Badge { get; set; }

        public virtual int ReminderAllCount { get; set; }

        public virtual int ReminderMeCount { get; set; }

        public virtual double Sorting { get; set; }
    }
}
