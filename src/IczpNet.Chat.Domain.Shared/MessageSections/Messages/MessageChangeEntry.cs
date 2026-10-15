using System;
using System.Text.Json.Serialization;

namespace IczpNet.Chat.MessageSections.Messages;

public enum MessageChangeKind { Rollback = 1, Delete = 2, Update = 3, Clear = 4 }

/// <summary>Content-free durable change metadata. Visibility is checked before returning it.</summary>
public class MessageChangeEntry
{
    [JsonIgnore] public bool AlreadyApplied { get; set; }
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long MessageId { get; set; }
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long Version { get; set; }
    public MessageChangeKind ChangeType { get; set; }
    public DateTime ChangedAt { get; set; }
    public DateTime CreationTime { get; set; }
    public bool IsPrivate { get; set; }
    public Guid? SenderSessionUnitId { get; set; }
    public Guid? ReceiverSessionUnitId { get; set; }
}

public record MessageChangeCheckpoint(Guid Epoch, long Version, long MinVersion);

public class MessageSyncItem
{
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long MessageId { get; set; }
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long SyncVersion { get; set; }
    public bool IsRollbacked { get; set; }
    public DateTime? RollbackTime { get; set; }
}
