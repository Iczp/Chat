using IczpNet.Chat.Bases;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.OpenedRecorders
{
    public class OpenedRecorderManager : RecorderManager<OpenedRecorder>, IOpenedRecorderManager
    {
        public OpenedRecorderManager(IRepository<OpenedRecorder> repository) : base(repository)
        {

        }

        protected override OpenedRecorder CreateEntity(SessionUnit entity, Message message, string deviceId)
        {
            return new OpenedRecorder(entity, message.Id, deviceId);
        }
    }
}
