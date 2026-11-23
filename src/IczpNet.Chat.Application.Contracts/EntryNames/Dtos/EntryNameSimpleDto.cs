using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.EntryNames.Dtos;

public class EntryNameSimpleDto : EntityDto<Guid>
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual string InputType { get; set; }

    public virtual string Regex { get; set; }

    public virtual string ErrorMessage { get; set; }

    public virtual bool IsStatic { get; set; }

    public virtual bool IsChoice { get; set; }

    public virtual bool IsRequired { get; set; }

    public virtual int? MaxLenth { get; set; }

    public virtual int? MinLenth { get; set; }
}
