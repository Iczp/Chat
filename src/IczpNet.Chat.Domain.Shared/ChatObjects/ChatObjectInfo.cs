using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectInfo
    {
        public virtual Guid Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string Code { get; set; }

        public virtual string Portrait { get; set; }

        public virtual Guid? AppUserId { get; set; }

        public virtual ChatObjectTypes? ObjectType { get; set; }
    }
}
