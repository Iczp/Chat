using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.EntryValues.Dtos;

public class EntryValueGetListInput : BaseGetListInput
{
    public virtual Guid? EntryNameId { get; set; }
}
