using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections
{
    public interface IChannelResolver
    {
        Task<Channels> GetAsync(ChatObject sender, ChatObject receiver);

        Channels Get(ChatObject sender, ChatObject receiver);
    }
}
