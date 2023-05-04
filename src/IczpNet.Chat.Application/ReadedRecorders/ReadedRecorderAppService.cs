using IczpNet.Chat.BaseAppServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.ReadedRecorders
{
    public class ReadedRecorderAppService : ChatAppService, IReadedRecorderAppService
    {

        protected IReadedRecorderManager ReadedRecorderManager { get; }

        public ReadedRecorderAppService(IReadedRecorderManager readedRecorderManager)
        {
            ReadedRecorderManager = readedRecorderManager;
        }

        public Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList)
        {
            return ReadedRecorderManager.GetCountsAsync(messageIdList);
        }
    }
}
