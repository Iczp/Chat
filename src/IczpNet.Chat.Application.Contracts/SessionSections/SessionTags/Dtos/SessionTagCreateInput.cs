using System;

namespace IczpNet.Chat.SessionSections.SessionTags.Dtos;

public class SessionTagCreateInput : SessionTagUpdateInput
{
    public virtual Guid SessionId { get; set; }
}
