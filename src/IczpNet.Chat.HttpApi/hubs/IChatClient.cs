using IczpNet.Pusher.Models;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Hubs;

public interface IChatClient
{
    Task ReceivedMessage(PushPayload  pushPayload);
}
