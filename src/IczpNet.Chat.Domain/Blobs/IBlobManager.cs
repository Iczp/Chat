using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.Blobs;

public class BlobAdminListResult
{
    public long TotalCount { get; set; }

    public List<Blob> Items { get; set; } = new();
}

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

    Task<BlobAdminListResult> GetAdminListAsync(
        string keyword = null,
        string container = null,
        bool? isPublic = null,
        bool? isStatic = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default);

    Task<Blob> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Blob> UpdateAdminAsync(
        Guid id,
        string fileName,
        bool? isPublic,
        bool? isStatic,
        CancellationToken cancellationToken = default);

    Task DeleteAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
