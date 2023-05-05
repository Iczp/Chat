using IczpNet.Chat.Bases;
using IczpNet.Chat.SessionSections.SessionUnits;
using System.Threading.Tasks;

namespace IczpNet.Chat.OpenedRecorders
{
    public interface IOpenedRecorderManager : IRecorderManager
    {
        Task<OpenedRecorder> SetOpenedAsync(SessionUnit entity, long messageId, string deviceId);
    }
}
