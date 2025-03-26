using IczpNet.Pusher.ShortIds;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Blobs;

public class BlobResolver(IShortIdGenerator shortIdGenerator) : DomainService, IBlobResolver
{
    protected string DateDirectoryName => Clock.Now.ToString("yyyy/MM/dd");

    public IShortIdGenerator ShortIdGenerator { get; } = shortIdGenerator;

    public virtual async Task<string> GetDirectoryNameAsync(string container, string folder, Guid blobId, Guid sessionId, Guid sessionUnitId)
    {
        await Task.Yield();
        var directoryName = $"{DateDirectoryName}/{folder}/{sessionId}/{sessionUnitId}";
        return directoryName;
    }

    public virtual async Task<string> GetFileUrlAsync(Guid blobId)
    {
        await Task.Yield();
        return $"/file?id={blobId}";
    }

    public virtual async Task<string> GenerateFileNameAsync(string suffix)
    {
        await Task.Yield();
        var fileName = Clock.Now.ToString("yyyyMMddhhmmss") + "_" + ShortIdGenerator.Create() + suffix;
        return fileName.ToLower();
    }
}
