using System;

namespace IczpNet.Chat.EntryValues.Dtos;

public class EntryValueCreateInput : EntryValueUpdateInput
{
    public virtual Guid EntryNameId { get; set; }
}
