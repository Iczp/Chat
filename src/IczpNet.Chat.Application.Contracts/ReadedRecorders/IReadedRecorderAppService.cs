using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IczpNet.Chat.ReadedRecorders
{
    public interface IReadedRecorderAppService
    {
        Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList);
    }
}
