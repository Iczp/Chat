using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.InvitationCodes.Dtos;

public class InvitationCodeDto : BaseDto<Guid>
{
    public virtual string Title { get; set; }

    public virtual long? OwnerId { get; set; }
}
