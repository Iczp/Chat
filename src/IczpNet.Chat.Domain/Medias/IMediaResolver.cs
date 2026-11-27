using System.Threading.Tasks;

namespace IczpNet.Chat.Medias;

public sealed record VideoProcessOptions(
    bool IncludeSnapshot = true,
    bool IncludeGif = true);

public interface IMediaResolver
{
    Task<VideoInfo> GetVideoInfoAsync(byte[] bytes, string fileName);

    Task<VideoInfo> GetVideoInfoAsync(byte[] bytes, string fileName, VideoProcessOptions options)
        => GetVideoInfoAsync(bytes, fileName);

    Task<AudioInfo> GetAudioInfoAsync(byte[] bytes, string fileName);
}
