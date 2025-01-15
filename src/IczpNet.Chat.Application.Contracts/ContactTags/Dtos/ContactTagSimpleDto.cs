using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ContactTags.Dtos;

public class ContactTagSimpleDto : EntityDto<Guid>
{
    public virtual string Name { get; set; }
}
