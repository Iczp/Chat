using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections
{
    public class ChannelResolver : DomainService, IChannelResolver
    {
        public Channels Get(IChatObject sender, IChatObject receiver)
        {
            if (sender.ObjectType == ChatObjectTypeEnums.Room || receiver.ObjectType == ChatObjectTypeEnums.Room)
            {
                return Channels.RoomChannel;
            }
            if (sender.ObjectType == ChatObjectTypeEnums.Official || receiver.ObjectType == ChatObjectTypeEnums.Official)
            {
                return Channels.ServiceChannel;
            }
            if (sender.ObjectType == ChatObjectTypeEnums.Square || receiver.ObjectType == ChatObjectTypeEnums.Square)
            {
                return Channels.SquareChannel;
            }
            return Channels.PrivateChannel;
        }

        public Task<Channels> GetAsync(IChatObject sender, IChatObject receiver)
        {
            return Task.FromResult(Get(sender, receiver));
        }
    }
}
