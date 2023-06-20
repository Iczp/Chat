using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.EntryValues.Dtos;

public class EntryValueGetListInput : GetListInput
{
    public virtual Guid? EntryNameId { get; set; }

    public virtual bool? IsStatic { get; set; }

    public virtual bool? IsPublic { get; set; }

    public virtual bool? IsOption { get; set; }
}
