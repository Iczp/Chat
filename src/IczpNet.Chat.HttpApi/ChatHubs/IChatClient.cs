using IczpNet.Pusher.Models;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatHubs;

public interface IChatClient
{
    Task ReceivedMessage(PushPayload  pushPayload);

    //Task StopAsync(string connectionId);
}
