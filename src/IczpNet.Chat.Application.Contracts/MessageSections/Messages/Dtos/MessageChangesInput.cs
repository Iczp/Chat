using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageChangesInput
{
    [Required, MinLength(1), MaxLength(20)]
    public List<MessageChangeScopeInput> Sessions { get; set; } = [];
    [Range(1, 200)] public int MaxResultCount { get; set; } = 100;
}

public class MessageChangeScopeInput
{
    public Guid SessionUnitId { get; set; }
    [MaxLength(512)] public string Cursor { get; set; }
    [MaxLength(512)] public string SnapshotCursor { get; set; }
}

public class MessageSyncItemsInput
{
    public Guid SessionUnitId { get; set; }
    [Required, MinLength(1), MaxLength(200)]
    public List<long> MessageIds { get; set; } = [];
}

public class MessageChangePageDto
{
    public string ErrorCode { get; set; }
    public Guid SessionUnitId { get; set; }
    public List<MessageSyncChangeDto> Items { get; set; } = [];
    public string NextCursor { get; set; }
    public string SnapshotCursor { get; set; }
    public bool HasMore { get; set; }
    public bool ResetRequired { get; set; }
}

public class MessageSyncChangeDto : MessageSyncItem
{
    public MessageChangeKind ChangeType { get; set; }
    public DateTime ChangedAt { get; set; }
}
