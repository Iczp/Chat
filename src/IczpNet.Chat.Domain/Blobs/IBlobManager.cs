using System;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.Blobs;

public interface IBlobManager
{
    Task SaveBytesAsync(string container, string name, byte[] bytes);

    Task<byte[]> GetBytesAsync(string container, string name);

    Task<byte[]> GetBytesAsync(Guid id);

    Task<(string Container, string StorageName)> ResolvePhysicalLocationAsync(Blob blob, CancellationToken cancellationToken = default);

    Task<BlobObject> FindStorageObjectAsync(Guid storageObjectId, CancellationToken cancellationToken = default);

    Task<Blob> CreateAsync(Blob blob);

    Task<Blob> GetEntityAsync(string container, string name);

    Task<Blob> GetAsync(Guid id);

    Task<Blob> FindAsync(Guid id);
}
