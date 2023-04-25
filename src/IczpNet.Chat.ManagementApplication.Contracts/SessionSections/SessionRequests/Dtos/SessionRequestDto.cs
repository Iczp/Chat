using IczpNet.Chat.Management.BaseDtos;
using System;
using IczpNet.Chat.Management.ChatObjects.Dtos;

namespace IczpNet.Chat.Management.SessionSections.SessionRequests.Dtos;

public class SessionRequestDto : BaseDto<Guid>
{
    public virtual long OwnerId { get; set; }

    //public virtual long? DestinationId { get; set; }

    //public virtual ChatObjectSimpleDto Owner { get; set; }

    public virtual ChatObjectSimpleDto Destination { get; set; }

    public virtual bool IsHandled { get; set; }

    public virtual bool? IsAgreed { get; set; }

    public virtual string RequestMessage { get; set; }

    public virtual bool IsExpired { get; set; }

    public virtual DateTime? ExpirationTime { get; set; }

}
