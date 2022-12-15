using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.ChatUsers
{
    public class ChatUser : ChatObject
    {
        public const ChatObjectTypes ChatObjectTypeValue = ChatObjectTypes.Personal;

        public override Guid? AppUserId { get; protected set; }
        protected ChatUser()
        {
            ObjectType = ChatObjectTypeValue;
        }

        protected ChatUser(Guid id) : base(id, ChatObjectTypeValue) { }
    }
}
