using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Blobs;

public interface IBlobManager
{
    Task SaveBytesAsync(string container, string name, byte[] bytes);

    Task<byte[]> GetBytesAsync(string container, string name);

    Task<Blob> CreateAsync(Blob blob);

    Task<Blob> GetEntityAsync(string container, string name);

    Task<Blob> GetAsync(Guid id);

    Task<Blob> FindAsync(Guid id);
}
