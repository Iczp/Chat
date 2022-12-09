using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections
{
    public class SessionIdGenerator : DomainService, ISessionIdGenerator
    {
        public string Make(MessageChannels messageChannel, ChatObject sender, ChatObject receiver)
        {
            string sessionId = null;
            switch (messageChannel)
            {
                case MessageChannels.RoomChannel:
                case MessageChannels.SubscriptionChannel:
                case MessageChannels.ServiceChannel:
                case MessageChannels.SquareChannel:
                    sessionId = receiver.Id.ToString();
                    break;
                case MessageChannels.PersonalToPersonal:
                case MessageChannels.RobotChannel:
                    var arr = new[] { sender.Id, receiver.Id };
                    Array.Sort(arr);
                    sessionId = string.Join(":", arr);
                    break;

                case MessageChannels.ElectronicCommerceChannel:
                    break;
            }
            return sessionId;
        }

        public Task<string> MakeAsync(MessageChannels messageChannel, ChatObject sender, ChatObject receiver)
        {
            return Task.FromResult(Make(messageChannel, sender, receiver));
        }
    }
}
