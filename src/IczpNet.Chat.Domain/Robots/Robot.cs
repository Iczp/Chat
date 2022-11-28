using IczpNet.Chat.ChatObjects;
using System;
using IczpNet.Chat.Enums;
namespace IczpNet.Chat.Robots
{
    public class Robot : ChatObject
    {
        public const ChatObjectTypeEnum ChatObjectTypeValue = ChatObjectTypeEnum.Robot;

        protected Robot()
        {
            ChatObjectType = ChatObjectTypeValue;
        }
        protected Robot(Guid id) : base(id, ChatObjectTypeValue) { }
    }
}
