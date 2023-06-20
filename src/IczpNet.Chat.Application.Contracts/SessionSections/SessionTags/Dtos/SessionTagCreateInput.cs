using System;

namespace IczpNet.Chat.SessionSections.SessionTags.Dtos;

public class SessionTagCreateInput : SessionTagUpdateInput
{
    /// <summary>
    /// 会话Id
    /// </summary>
    public virtual Guid SessionId { get; set; }
}
