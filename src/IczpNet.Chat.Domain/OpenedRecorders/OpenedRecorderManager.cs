using IczpNet.AbpCommons;
using IczpNet.Chat.Bases;
using IczpNet.Chat.SessionSections.SessionUnits;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.OpenedRecorders
{
    public class OpenedRecorderManager : RecorderManager<OpenedRecorder>, IOpenedRecorderManager
    {
        public OpenedRecorderManager(IRepository<OpenedRecorder> repository) : base(repository)
        {

        }

        public virtual async Task<OpenedRecorder> SetOpenedAsync(SessionUnit entity, long messageId, string deviceId)
        {
            var message = await MessageRepository.GetAsync(messageId);

            Assert.If(entity.SessionId != message.SessionId, $"Not in same session,messageId:{messageId}");

            var openedRecorder = await Repository.FindAsync(x => x.SessionUnitId == entity.Id && x.MessageId == messageId);

            if (openedRecorder == null)
            {
                return await Repository.InsertAsync(new OpenedRecorder(entity, messageId, deviceId), autoSave: true);
            }

            return openedRecorder;
        }
    }
}
