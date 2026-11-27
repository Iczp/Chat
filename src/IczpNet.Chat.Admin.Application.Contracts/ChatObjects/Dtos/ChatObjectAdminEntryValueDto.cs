using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Admin.ChatObjects.Dtos;

public class ChatObjectAdminEntryValueDto
{
    public Guid EntryNameId { get; set; }

    public string EntryName { get; set; }

    public Guid EntryValueId { get; set; }

    public string Value { get; set; }
}
