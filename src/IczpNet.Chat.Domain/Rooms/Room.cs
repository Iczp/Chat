using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Rooms
{
    public class Room : ChatObject
    {

        public virtual Guid? ChatObjectId { get; set; }

        [ForeignKey(nameof(ChatObjectId))]
        public virtual ChatObject ChatObject { get; set; }

        protected Room()
        {
            ChatObjectType = ChatObjectType.Room;
        }
        protected Room(Guid id) : base(id, ChatObjectType.Room)
        {

        }

    }
}
