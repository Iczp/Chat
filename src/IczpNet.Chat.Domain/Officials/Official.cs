using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;
namespace IczpNet.Chat.Officials
{
    public class Official : ChatObject
    {
        public const ChatObjectType ChatObjectTypeValue = ChatObjectType.Official;

        protected Official()
        {
            ChatObjectType = ChatObjectTypeValue;
        }
        protected Official(Guid id) : base(id, ChatObjectTypeValue) { }
    }
}
