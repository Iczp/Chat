using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionBoxes;
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

    /// <summary>
    /// 消息盒子
    /// </summary>
    public List<BoxCountDto> Boxes { get; set; }
}
