using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ContactTags.Dtos;

public class ContactTagDto : EntityDto<Guid>
{
    public virtual string Name { get; set; }

    public virtual string Index { get; set; }

    public virtual long OwnerId { get; set; }
}
