using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.Blobs;

public interface IVideoGifPropagationOutboxService
{
    Task<int> ProcessPendingAsync(int batchSize = 50, CancellationToken cancellationToken = default);
}
