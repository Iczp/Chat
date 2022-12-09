using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageSections
{
    public class MessageChannelGenerator : DomainService, IMessageChannelGenerator
    {
        public MessageChannels Make(ChatObject sender, ChatObject receiver)
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

        public Task<MessageChannels> MakeAsync(ChatObject sender, ChatObject receiver)
        {
            return Task.FromResult(Make(sender, receiver));
        }
    }
}
