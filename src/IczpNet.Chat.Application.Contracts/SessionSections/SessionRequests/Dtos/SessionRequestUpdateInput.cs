using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.SessionSections.SessionRequests.Dtos;

public class SessionRequestUpdateInput : BaseInput
{
    public virtual string RequestMessage { get; set; }
}
