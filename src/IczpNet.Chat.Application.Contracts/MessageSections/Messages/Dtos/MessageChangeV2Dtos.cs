using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageChangeHeadsInput
{
    [Range(1, long.MaxValue)]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long ChatObjectId { get; set; }

    [StringLength(200)]
    public string DeviceId { get; set; }

    [Required, MinLength(1), MaxLength(200)]
    public List<Guid> SessionUnitIds { get; set; }
}

public class MessageChangeV2ScopeInput
{
    [Range(1, long.MaxValue)]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long ChatObjectId { get; set; }

    [StringLength(200)]
    public string DeviceId { get; set; }

    public Guid SessionUnitId { get; set; }
}

public class MessageChangeIncrementalInput : MessageChangeV2ScopeInput
{
    [Required, StringLength(64)]
    public string Epoch { get; set; }

    [Required, RegularExpression("^[0-9]{1,19}$")]
    public string ChangeSeq { get; set; }

    [Range(1, 200)]
    public int MaxResultCount { get; set; } = 100;
}

public class MessageByIdsInput : MessageChangeV2ScopeInput
{
    [Required, MinLength(1), MaxLength(200)]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public List<long> MessageIds { get; set; }
}

public record MessageChangeHeadDto(Guid SessionUnitId, string Epoch, string HeadSeq, string MinSeq,
    DateTime? ClearTime, string ErrorCode = null);

public record MessageChangeHeadsDto(List<MessageChangeHeadDto> Items);

public record MessageChangeV2ItemDto(string MessageId, string ChangeSeq, string ChangeType,
    DateTime? ClearTime, DateTime? RollbackTime);

public record MessageChangeIncrementalDto(string Epoch, string HeadSeq, string MinSeq, string NextSeq,
    bool HasMore, bool ResetRequired, List<MessageChangeV2ItemDto> Items);

public record MessageByIdDto(string MessageId, string Status, string ChangeSeq, MessageFastDto Message,
    DateTime? RollbackTime);

public record MessageByIdsDto(List<MessageByIdDto> Items);
