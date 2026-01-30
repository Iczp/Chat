using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System.Threading.Tasks;

namespace IczpNet.Chat.Sessions;

public interface IChannelResolver
{
    Task<Channels> GetAsync(IChatObject sender, IChatObject receiver);

    Channels Get(IChatObject sender, IChatObject receiver);
}
