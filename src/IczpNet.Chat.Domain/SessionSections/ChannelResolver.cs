using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections
{
    public class ChannelResolver : DomainService, IChannelResolver
    {
        public Channels Get(ChatObjectInfo sender, ChatObjectInfo receiver)
        {
            if (sender.ObjectType == ChatObjectTypes.Room || receiver.ObjectType == ChatObjectTypes.Room)
            {
                return Channels.RoomChannel;
            }
            if (sender.ObjectType == ChatObjectTypes.Official || receiver.ObjectType == ChatObjectTypes.Official)
            {
                return Channels.ServiceChannel;
            }
            if (sender.ObjectType == ChatObjectTypes.Square || receiver.ObjectType == ChatObjectTypes.Square)
            {
                return Channels.SquareChannel;
            }
            return Channels.PrivateChannel;
        }

        public Task<Channels> GetAsync(ChatObjectInfo sender, ChatObjectInfo receiver)
        {
            return Task.FromResult(Get(sender, receiver));
        }
    }
}
