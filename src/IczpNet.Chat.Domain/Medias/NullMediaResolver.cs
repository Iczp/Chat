using IczpNet.Chat.Options;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Medias;

public class NullMediaResolver : DomainService, IMediaResolver
{
    protected MessageOptions Options => LazyServiceProvider.LazyGetRequiredService<IOptions<MessageOptions>>().Value;

    public async Task<AudioInfo> GetAudioInfoAsync(byte[] bytes, string fileName)
    {
        await Task.Yield();
        return null;
    }

    public virtual Task<VideoInfo> GetVideoInfoAsync(byte[] bytes, string fileName)
    {
        throw new System.NotImplementedException();
    }

    protected virtual async Task<Stream> CreateTmpAsync(byte[] bytes)
    {
        var filename = $"{GuidGenerator.Create()}.tmp";

        string filePath = Path.Combine(Options.OutputTempPath, filename);

        return await CreateTmpAsync(bytes, filePath);
    }

    protected virtual async Task<Stream> CreateTmpAsync(byte[] bytes, string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

        await fileStream.WriteAsync(bytes);

        return fileStream;
    }

    protected virtual async Task<bool> RemoveTmpAsync(string filePath)
    {
        await Task.Yield();
        return false;
    }
}
