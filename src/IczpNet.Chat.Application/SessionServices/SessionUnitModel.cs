using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using System;

namespace IczpNet.Chat.SessionServices
{
    public class SessionUnitModel
    {
        public virtual Guid Id { get; set; }

        public virtual Guid SessionId { get; set; }

        public virtual Guid OwnerId { get; set; }

        public virtual string Rename { get; set; }

        public virtual ChatObject Destination { get; set; }

        public virtual Message LastMessage { get; set; }

        public virtual long? LastMessageAutoId { get; set; }

        public virtual int Badge { get; set; }

        public virtual bool IsImmersed { get; set; }

        public virtual bool IsImportant { get; set; }

        public virtual long ReadedMessageAutoId { get; set; }

        public virtual int ReminderAllCount { get; set; }

        public virtual int ReminderMeCount { get; set; }

        public virtual double Sorting { get; set; }
    }
}
