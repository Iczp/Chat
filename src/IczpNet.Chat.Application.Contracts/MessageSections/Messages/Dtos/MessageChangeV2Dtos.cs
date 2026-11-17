using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageChangeHeadInput
{
    [Range(1, long.MaxValue)]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long ChatObjectId { get; set; }

    [Required, StringLength(200)]
    public string DeviceId { get; set; }
}

public record MessageChangeHeadV2Dto(
    string Epoch,
    string HeadChangeId,
    string LastAppliedChangeId,
    bool Initialized);

public class MessageChangePullInput
{
    [Range(1, long.MaxValue)]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long ChatObjectId { get; set; }

    [Required, StringLength(200)]
    public string DeviceId { get; set; }

    [Required, StringLength(64)]
    public string Epoch { get; set; }

    [Required, RegularExpression("^[0-9]{1,19}$")]
    public string AfterMessageChangeId { get; set; }

    [Range(1, 200)]
    public int MaxResultCount { get; set; } = 100;
}

public record MessageChangePullItemDto(
    string MessageChangeId,
    Guid SessionId,
    Guid SessionUnitId,
    string MessageId,
    string ChangeType);

public record MessageChangePullDto(
    string Epoch,
    string HeadChangeId,
    string NextCursor,
    bool HasMore,
    bool ResetRequired,
    List<MessageChangePullItemDto> Items);

public class MessageChangeAckInput
{
    [Range(1, long.MaxValue)]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long ChatObjectId { get; set; }

    [Required, StringLength(200)]
    public string DeviceId { get; set; }

    [Required, StringLength(64)]
    public string Epoch { get; set; }

    [Required, RegularExpression("^[0-9]{1,19}$")]
    public string LastAppliedMessageChangeId { get; set; }

    public bool Reset { get; set; }
}

public record MessageChangeAckDto(
    string Epoch,
    string HeadChangeId,
    string LastAppliedChangeId,
    bool Initialized,
    bool ResetRequired);

public class MessageByIdsScopeRequest
{
    public Guid SessionUnitId { get; set; }

    [Required, MinLength(1), MaxLength(200)]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public List<long> MessageIds { get; set; } = [];
}

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

public record MessageByIdDto(
    Guid SessionUnitId,
    string MessageId,
    string Status,
    string MessageChangeId,
    int? EntityVersion,
    string ChangeSeq,
    MessageFastDto Message,
    DateTime? RollbackTime);

public record MessageByIdsDto(List<MessageByIdDto> Items);
