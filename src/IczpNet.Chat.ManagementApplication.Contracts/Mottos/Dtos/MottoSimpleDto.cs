using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Management.Mottos.Dtos;

public class MottoSimpleDto : EntityDto<Guid>
{
    public virtual string Title { get; set; }
}
