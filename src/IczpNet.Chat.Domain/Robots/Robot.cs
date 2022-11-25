using IczpNet.Chat.ChatObjects;
using System;
using IczpNet.Chat.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Robots
{
    public class Robot : ChatObject
    {
        //public virtual Guid? ChatObjectId { get; set; }

        //[ForeignKey(nameof(ChatObjectId))]
        //public virtual ChatObject ChatObject { get; set; }

        protected Robot()
        {
            ChatObjectType = ChatObjectType.Room;
        }
        protected Robot(Guid id) : base(id, ChatObjectType.Robot)
        {

        }
    }
}
