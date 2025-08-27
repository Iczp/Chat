using IczpNet.Chat.CommandPayloads;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatHubs;

public interface IChatClient
{
    Task ReceivedMessage(CommandPayload pushPayload);

    //Task StopAsync(string connectionId);
}
