using System.Collections.Generic;

namespace IczpNet.Chat.ConnectionPools;

public class AbortEto
{
    public List<string> ConnectionIdList { get; set; }

    public string Reason { get; set; }
}
