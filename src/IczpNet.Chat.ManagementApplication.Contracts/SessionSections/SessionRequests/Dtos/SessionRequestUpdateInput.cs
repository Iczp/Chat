using IczpNet.Chat.Management.BaseDtos;

namespace IczpNet.Chat.Management.SessionSections.SessionRequests.Dtos;

public class SessionRequestUpdateInput : BaseInput
{
    public virtual string RequestMessage { get; set; }
}
