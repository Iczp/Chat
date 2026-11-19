using System;
using System.Text.Json.Serialization;

namespace IczpNet.Chat.MessageSections.Messages;

/// <summary>Content-free durable change metadata.</summary>
public class MessageChangeEntry
{
    public Guid SessionId { get; set; }
    [JsonIgnore] public bool AlreadyApplied { get; set; }
    [JsonIgnore] public long SessionSeq { get; set; }
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long MessageId { get; set; }
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long Version { get; set; }
    public MessageChangeKind ChangeType { get; set; }
    public string Description { get; set; }
    public DateTime ChangedAt { get; set; }
    public DateTime CreationTime { get; set; }
    public bool IsPrivate { get; set; }
    public Guid? SenderSessionUnitId { get; set; }
    public Guid? ReceiverSessionUnitId { get; set; }
}
