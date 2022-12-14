using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections
{
    public interface IMessageChannelResolver
    {
        Task<MessageChannels> MakeAsync(ChatObject sender, ChatObject receiver);

        MessageChannels Make(ChatObject sender, ChatObject receiver);
    }
}
