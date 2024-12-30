namespace IczpNet.Chat.Pusher;

public interface IPusherFactory
{
    /// <summary>
    /// The container object.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IPusherCommand Create(string name);
}
