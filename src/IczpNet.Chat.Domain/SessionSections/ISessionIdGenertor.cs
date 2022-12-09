using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections
{
    public interface ISessionIdGenerator
    {
        Task<string> MakeAsync(MessageChannels messageChannel, ChatObject sender, ChatObject receiver);
        string Make(MessageChannels messageChannel, ChatObject sender, ChatObject receiver);
    }
}
