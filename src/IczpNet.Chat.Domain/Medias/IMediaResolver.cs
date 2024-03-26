using System.Threading.Tasks;

namespace IczpNet.Chat.Medias;

public interface IMediaResolver
{
    //Task<VideoInfo> GetVideoInfoAsync(string videoPath);

    Task<VideoInfo> GetVideoInfoAsync(byte[] bytes, string fileName);

    //Task<AudioInfo> GetAudioInfoAsync(string audioPath);

    Task<AudioInfo> GetAudioInfoAsync(byte[] bytes, string fileName);
}
