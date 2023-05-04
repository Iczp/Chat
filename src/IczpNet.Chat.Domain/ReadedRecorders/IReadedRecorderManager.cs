using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.ReadedRecorders
{
    public interface IReadedRecorderManager
    {
        Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList);
    }
}
