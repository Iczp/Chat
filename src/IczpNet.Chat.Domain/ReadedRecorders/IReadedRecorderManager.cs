using IczpNet.Chat.Bases;
using IczpNet.Chat.SessionSections.SessionUnits;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.ReadedRecorders
{
    public interface IReadedRecorderManager : IRecorderManager
    {

        Task<int> SetReadedManyAsync(SessionUnit entity, List<long> messageIdList, string deviceId);
    }
}
