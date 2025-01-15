using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Words.Dtos;

public class WordDto : EntityDto<Guid>
{
    public virtual string Value { get; set; }

    public virtual bool IsEnabled { get; set; }

    public virtual bool IsDirty { get; set; }
}
