using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageSections
{
    public class ChannelResolver : DomainService, IChannelResolver
    {
        public MessageChannels Get(ChatObject sender, ChatObject receiver)
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

        public Task<MessageChannels> GetAsync(ChatObject sender, ChatObject receiver)
        {
            return Task.FromResult(Get(sender, receiver));
        }
    }
}
