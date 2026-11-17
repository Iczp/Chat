using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageByIdsScopeRequest
{
    public Guid SessionUnitId { get; set; }

    [Required, MinLength(1), MaxLength(200)]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public List<long> MessageIds { get; set; } = [];
}
