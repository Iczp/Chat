using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.ConnectionPools;

public interface IAbortService
{
    Task AbortAsync(string connectionId, string reason);

    Task AbortAsync(List<string> connectionIdList, string reason);

}
