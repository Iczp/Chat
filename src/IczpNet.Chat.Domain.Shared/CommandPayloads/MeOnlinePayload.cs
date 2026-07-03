using IczpNet.Chat.ConnectionPools;
using System.Collections.Generic;

namespace IczpNet.Chat.CommandPayloads;

public class MeOnlinePayload
{
    public string Current { get; set; }
    public List<ConnectionPoolCacheItem> Connections { get; set; }
}
