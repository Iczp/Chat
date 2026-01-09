using IczpNet.Chat.Enums;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class FriendCountDto
{
    /// <summary>
    /// 总数
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Dictionary<ChatObjectTypeEnums, long> CountMap { get; set; }
}
