using System;

namespace IczpNet.Chat.Management.SessionSections.SessionTags.Dtos;

public class SessionTagCreateInput : SessionTagUpdateInput
{
    public virtual Guid SessionId { get; set; }
}
