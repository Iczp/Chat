using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IczpNet.Chat.MessageSections.Messages;

/// <summary>A database-certified, member-independent open interval of message metadata.</summary>
public class SessionMessageWindowCacheItem
{
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long MinMessageId { get; set; }
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long MaxMessageId { get; set; } = long.MaxValue;
    public List<Entry> Items { get; set; } = [];

    public class Entry
    {
        [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
        public long Id { get; set; }
        public bool IsPrivate { get; set; }
        public Guid? SenderSessionUnitId { get; set; }
        public Guid? ReceiverSessionUnitId { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
