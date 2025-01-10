using System.Threading.Tasks;

namespace IczpNet.Chat.Pusher;

public interface IPusherCommand
{
    Task SendToAsync(string message);
}
