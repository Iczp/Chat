using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.SessionSections.SessionRequests.Dtos;

public class SessionRequestCreateInput : BaseInput
{
    public virtual long OwnerId { get; set; }

    public virtual long DestinationId { get; set; }

    public virtual string RequestMessage { get; set; }

}
