using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ReadedRecorders
{
    public class ReadedRecorderManager : DomainService, IReadedRecorderManager
    {
        protected IRepository<ReadedRecorder> Repository { get; }

        public ReadedRecorderManager(IRepository<ReadedRecorder> repository)
        {
            Repository = repository;
        }

        public virtual async Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList)
        {
            var dict = (await Repository.GetQueryableAsync())
                .Where(x => messageIdList.Contains(x.MessageId))
                .GroupBy(x => x.MessageId)
                .ToDictionary(x => x.Key, x => x.Count());

            foreach (var messageId in messageIdList.Except(dict.Keys))
            {
                dict[messageId] = 0;
            }

            return dict;
        }
    }
}
