using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.Rooms
{
    public class Room : ChatObject
    {

        public const ChatObjectType ChatObjectTypeValue = ChatObjectType.Room;

        protected Room()
        {
            ChatObjectType = ChatObjectTypeValue;
        }
        protected Room(Guid id) : base(id, ChatObjectTypeValue)
        {

        }

    }
}
