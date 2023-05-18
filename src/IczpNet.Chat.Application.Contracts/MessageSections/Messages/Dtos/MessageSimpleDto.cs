using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos
{
    public class MessageSimpleDto :  IEntityDto<long>
    {
        public virtual long Id { get; set; }

        public virtual Guid SessionId { get; set; }

        public virtual long? SenderId { get; set; }

        public virtual long? ReceiverId { get; set; }

        public virtual MessageTypes MessageType { get; set; }

        public virtual ReminderTypes? ReminderType { get; set; }

        public virtual string KeyName { get; set; }

        public virtual string KeyValue { get; set; }

        public virtual bool IsPrivate { get; set; }

        public virtual int SessionUnitCount { get; set; }

        public virtual int ReadedCount { get; set; }

        public virtual int OpenedCount { get; set; }

        public virtual int FavoritedCount { get; set; }

        public virtual DateTime? RollbackTime { get; set; }

        public virtual DateTime CreationTime { get; set; }
    }
}
