using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.MessageSections
{
    public static class MessageExtentions
    {
        public static MessageChannels MakeMessageChannel(ChatObject sender, ChatObject receiver)
        {
            if (sender.ObjectType == ChatObjectTypes.Room || receiver.ObjectType == ChatObjectTypes.Room)
            {
                return MessageChannels.RoomChannel;
            }
            else if (sender.ObjectType == ChatObjectTypes.Official || receiver.ObjectType == ChatObjectTypes.Official)
            {
                return MessageChannels.ServiceChannel;
            }
            else if (sender.ObjectType == ChatObjectTypes.Square || receiver.ObjectType == ChatObjectTypes.Square)
            {
                return MessageChannels.SquareChannel;
            }

            return MessageChannels.PersonalToPersonal;
        }

        public static string MakeSessionId(MessageChannels messageChannel, ChatObject Sender, ChatObject Receiver, bool IsReverse = false)
        {
            string sessionId = null;
            switch (messageChannel)
            {
                case MessageChannels.RoomChannel:
                case MessageChannels.SubscriptionChannel:
                case MessageChannels.ServiceChannel:
                case MessageChannels.SquareChannel:
                    sessionId = Receiver.Id.ToString();
                    break;
                case MessageChannels.PersonalToPersonal:
                case MessageChannels.RobotChannel:
                    var arr = new[] { Sender.Id, Receiver.Id };
                    Array.Sort(arr);
                    sessionId = string.Join(":", arr);
                    break;

                case MessageChannels.ElectronicCommerceChannel:
                    break;
            }
            return sessionId;
        }


    }
}
