using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Management.SessionSections.SessionTags.Dtos;

public class SessionTagSimpleDto : EntityDto<Guid>
{
    public virtual string Name { get; set; }
}
