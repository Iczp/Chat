using System.Collections.Generic;

namespace IczpNet.Chat.ConnectionPools.Dtos;

public class AbortInput
{
    /// <summary>
    /// ConnectionId
    /// </summary>
    public List<string> ConnectionIdList { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Reason { get; set; }

}
