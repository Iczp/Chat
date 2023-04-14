using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.SessionRequests.Dtos;

public class SessionRequestUpdateInput : BaseInput
{
    public virtual long OwnerId { get; set; }

    public virtual long DestinationId { get; set; }

    public virtual string RequestMessage { get; set; }
}
