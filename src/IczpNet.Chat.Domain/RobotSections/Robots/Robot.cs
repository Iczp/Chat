using IczpNet.Chat.ChatObjects;
using System;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.RobotSections.Robots
{
    public class Robot : ChatObject
    {
        public const ChatObjectTypeEnum ChatObjectTypeValue = ChatObjectTypeEnum.Robot;

        protected Robot()
        {
            ObjectType = ChatObjectTypeValue;
        }
        protected Robot(Guid id) : base(id, ChatObjectTypeValue) { }
    }
}
