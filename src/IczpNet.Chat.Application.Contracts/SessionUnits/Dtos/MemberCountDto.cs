using System;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class MemberCountDto
{
    /// <summary>
    /// 会话Id
    /// </summary>
    public Guid SessionId { get; set; }
    /// <summary>
    /// 总数
    /// </summary>
    public long TotalCount { get; set; }


}
