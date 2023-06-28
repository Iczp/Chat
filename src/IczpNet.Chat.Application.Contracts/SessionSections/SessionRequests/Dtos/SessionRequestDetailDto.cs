using IczpNet.Chat.ChatObjects.Dtos;
using System;

namespace IczpNet.Chat.SessionSections.SessionRequests.Dtos;

public class SessionRequestDetailDto : SessionRequestDto
{
    /// <summary>
    /// 发起者
    /// </summary>
    public override ChatObjectSimpleDto Owner { get; set; }

    /// <summary>
    /// 处理消息
    /// </summary>
    public virtual string HandleMessage { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    public virtual DateTime? HandleTime { get; set; }
}
