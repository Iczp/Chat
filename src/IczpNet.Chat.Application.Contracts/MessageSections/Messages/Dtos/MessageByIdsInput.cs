using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageByIdsInput
{
    [Range(1, long.MaxValue)]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long ChatObjectId { get; set; }

    [StringLength(200)]
    public string DeviceId { get; set; }

    public Guid? SessionUnitId { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public List<long> MessageIds { get; set; }

    public List<MessageByIdsScopeRequest> Requests { get; set; }
}
