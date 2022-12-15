using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections
{
    public class SessionGenerator : DomainService, ISessionGenerator
    {
        protected virtual string MakeSesssionId(MessageChannels messageChannel, ChatObject sender, ChatObject receiver)
        {
            switch (messageChannel)
            {
                case MessageChannels.RoomChannel:
                case MessageChannels.SubscriptionChannel:
                case MessageChannels.ServiceChannel:
                case MessageChannels.SquareChannel:
                    return receiver.Id.ToString();
                case MessageChannels.PersonalToPersonal:
                case MessageChannels.RobotChannel:
                    var arr = new[] { sender.Id, receiver.Id };
                    Array.Sort(arr);
                    return string.Join(":", arr);
                case MessageChannels.ElectronicCommerceChannel:
                default:
                    return null;
            }
        }

        public Task<string> MakeSesssionIdAsync(MessageChannels messageChannel, ChatObject sender, ChatObject receiver)
        {
            return Task.FromResult(MakeSesssionId(messageChannel, sender, receiver));
        }

        public Task<List<Session>> GenerateAsync(Guid ownerId, long startMessageAutoId)
        {
            throw new NotImplementedException();
        }
    }
}
