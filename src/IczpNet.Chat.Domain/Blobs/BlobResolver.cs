using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Blobs;

public class BlobResolver : DomainService, IBlobResolver
{
    protected string DateDirectoryName => Clock.Now.ToString("yyyy/MM/dd");


    public async Task<string> GetDirectoryNameAsync(BlobContext context)
    {
        await Task.Yield();
        var directoryName = $"{DateDirectoryName}/{context.Folder}/{context.SessionUnitId}/{context.SessionUnitId}";
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
        var randomFileName = Path.GetRandomFileName().Replace(".", "");
        var fileName = Clock.Now.ToString("yyyyMMddhhmmss") + "_" + randomFileName + suffix;
        return fileName.ToLower();
    }
}
