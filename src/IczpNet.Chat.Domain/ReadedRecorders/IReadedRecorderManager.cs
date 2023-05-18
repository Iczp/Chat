using IczpNet.Chat.Bases;
using System.Threading.Tasks;

namespace IczpNet.Chat.ReadedRecorders
{
    public interface IReadedRecorderManager : IRecorderManager<ReadedRecorder>
    {
        Task<int> CreateAllAsync(long messageId);
    }
}
