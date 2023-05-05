using IczpNet.Chat.Bases;
using IczpNet.Chat.SessionSections.SessionUnits;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ReadedRecorders
{
    public class ReadedRecorderManager : RecorderManager<ReadedRecorder>, IReadedRecorderManager
    {
        public ReadedRecorderManager(IRepository<ReadedRecorder> repository) : base(repository)
        {


        }

        public virtual async Task<int> SetReadedManyAsync(SessionUnit entity, List<long> messageIdList, string deviceId)
        {
            var dbMessageIdList = (await MessageRepository.GetQueryableAsync())
                .Where(x => x.SessionId == entity.SessionId && messageIdList.Contains(x.Id))
                .Select(x => x.Id)
                .ToList()
                ;

            if (!dbMessageIdList.Any())
            {
                return 0;
            }

            var recordedMessageIdList = (await Repository.GetQueryableAsync())
                .Where(x => x.SessionUnitId == entity.Id && dbMessageIdList.Contains(x.MessageId))
                .Select(x => x.MessageId)
                .ToList()
                ;

            var newMessageIdList = dbMessageIdList.Except(recordedMessageIdList)
                .Select(x => new ReadedRecorder(entity, x, deviceId))
                .ToList();

            if (newMessageIdList.Any())
            {
                await Repository.InsertManyAsync(newMessageIdList, autoSave: true);
            }


            //notice :IChatPusher.
            //...


            return newMessageIdList.Count;
        }
    }
}
