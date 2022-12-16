using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections
{
    public interface IChannelResolver
    {
        Task<MessageChannels> GetAsync(ChatObject sender, ChatObject receiver);

        MessageChannels Get(ChatObject sender, ChatObject receiver);
    }
}
