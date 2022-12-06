using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectInfo
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual ChatObjectTypes ObjectType { get; set; }
        public virtual string Portrait { get; set; }
    }
}
