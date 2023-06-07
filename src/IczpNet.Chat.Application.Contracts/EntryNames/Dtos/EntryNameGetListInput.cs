using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.EntryNames.Dtos;

public class EntryNameGetListInput : BaseTreeGetListInput<Guid>
{
    public virtual bool? IsChoice { get; set; }

    public virtual bool? IsUniqued { get; set; }

    public virtual bool? IsRequired { get; set; }

    public virtual bool? IsStatic { get; set; }

    public virtual bool? IsPublic { get; set; }
}
