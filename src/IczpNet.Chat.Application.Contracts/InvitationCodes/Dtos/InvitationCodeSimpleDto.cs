using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.InvitationCodes.Dtos;

public class InvitationCodeSimpleDto : EntityDto<Guid>
{
    public virtual string Title { get; set; }
}
