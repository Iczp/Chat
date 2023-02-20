using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections
{
    public interface IChannelResolver
    {
        Task<Channels> GetAsync(ChatObjectInfo sender, ChatObjectInfo receiver);

        Channels Get(ChatObjectInfo sender, ChatObjectInfo receiver);
    }
}
