using System.Net;

namespace IczpNet.Chat.Hosting;

public interface ICurrentHosted
{
    string Name { get; }

    IPAddress[] IPAddress { get; }
}
