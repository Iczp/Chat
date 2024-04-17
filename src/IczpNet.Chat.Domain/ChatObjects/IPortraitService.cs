using System.Threading.Tasks;

namespace IczpNet.Chat.ChatObjects;

public interface IPortraitService
{
    Task<byte[]> GetPortraitAsync(long chatObjectId, bool isThumbnail);
}
