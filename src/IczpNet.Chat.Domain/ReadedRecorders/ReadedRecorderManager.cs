using IczpNet.Chat.Bases;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ReadedRecorders
{
    public class ReadedRecorderManager : RecorderManager<ReadedRecorder>, IReadedRecorderManager
    {
        public ReadedRecorderManager(IRepository<ReadedRecorder> repository) : base(repository)
        {

        }

        protected override ReadedRecorder CreateEntity(SessionUnit sessionUnit, Message message, string deviceId)
        {
            return new ReadedRecorder(sessionUnit, message.Id, deviceId);
        }

    }
}
