using IczpNet.Chat.Management.BaseDtos;

namespace IczpNet.Chat.Management.SessionSections.SessionRequests.Dtos;

public class SessionRequestCreateInput : BaseInput
{
    public virtual long OwnerId { get; set; }

    public virtual long DestinationId { get; set; }

    public virtual string RequestMessage { get; set; }

}
