using System;

namespace IczpNet.Chat.SessionSections.SessionRequests.Dtos;

public class SessionRequestDetailDto : SessionRequestDto
{
    public virtual string HandleMessage { get; set; }

    public virtual DateTime? HandleTime { get; set; }
}
