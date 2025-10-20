using IczpNet.Chat.CommandPayloads;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatHubs;

public interface IChatClient
{
    Task ReceivedMessage(CommandPayload pushPayload);

    Task KickMessage(CommandPayload messagePayload);

    //Task StopAsync(string connectionId);
}
