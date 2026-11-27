using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Admin.ChatObjects.Dtos;

public class ChatObjectAdminDetailDto : ChatObjectAdminListItemDto
{
    public string NameSpelling { get; set; }

    public string NameSpellingAbbreviation { get; set; }

    public long MaxMessageAutoId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public List<Guid> CategoryIds { get; set; } = new();

    public List<string> CategoryNames { get; set; } = new();

    public List<ChatObjectAdminEntryValueDto> EntryValues { get; set; } = new();

    public int SameUserChatObjectCount { get; set; }
}
