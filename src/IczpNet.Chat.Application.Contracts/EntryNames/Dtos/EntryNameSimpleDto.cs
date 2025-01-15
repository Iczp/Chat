using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.EntryNames.Dtos;

public class EntryNameSimpleDto : EntityDto<Guid>
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }
}
