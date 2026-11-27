using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.Messages.Dtos;

public class MessageAdminGetListInput : PagedResultRequestDto
{
    public Guid? SessionId { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? SenderId { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? ReceiverId { get; set; }

    public MessageTypes? MessageType { get; set; }

    public bool? IsRollbacked { get; set; }

    public bool? IsPrivate { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? MaxMessageId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }
}

public class MessageAdminListItemDto : EntityDto<long>
{
    public Guid? SessionId { get; set; }

    public long SessionSeq { get; set; }

    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long? SenderId { get; set; }

    public string SenderName { get; set; }

    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long? ReceiverId { get; set; }

    public string ReceiverName { get; set; }

    public Channels Channel { get; set; }

    public MessageTypes MessageType { get; set; }

    public bool IsPrivate { get; set; }

    public bool IsRollbacked { get; set; }

    public DateTime? RollbackTime { get; set; }

    public long Size { get; set; }

    public DateTime CreationTime { get; set; }
}

public class MessageAdminDetailDto : MessageAdminListItemDto
{
    public string ClientMessageId { get; set; }

    public string ContentJson { get; set; }

    public Guid? SenderSessionUnitId { get; set; }

    public Guid? ReceiverSessionUnitId { get; set; }
}

public class MessageAdminGetHistoryInput
{
    [Required]
    public Guid SessionUnitId { get; set; }

    public int MaxResultCount { get; set; } = 20;

    public long? MaxMessageId { get; set; }
}

public class MessageAdminHistoryItemDto : EntityDto<long>
{
    public Guid SessionId { get; set; }

    public long? SenderId { get; set; }

    public string SenderName { get; set; }

    public string SenderDisplayName { get; set; }

    public Guid? SenderSessionUnitId { get; set; }

    public MessageTypes MessageType { get; set; }

    public string MessageTypeDescription { get; set; }

    public bool IsRollbacked { get; set; }

    public DateTime? RollbackTime { get; set; }

    public DateTime CreationTime { get; set; }

    public object Content { get; set; }
}

public class MessageAdminHistoryExtraDto
{
    public bool HasMore { get; set; }

    public long? NextCursorId { get; set; }
}

public class MessageAdminHistoryResultDto : PagedResultDto<MessageAdminHistoryItemDto>
{
    public MessageAdminHistoryExtraDto Extra { get; set; } = new();

    public MessageAdminHistoryResultDto()
    {
    }

    public MessageAdminHistoryResultDto(
        long totalCount,
        IReadOnlyList<MessageAdminHistoryItemDto> items,
        MessageAdminHistoryExtraDto extra)
        : base(totalCount, items)
    {
        Extra = extra ?? new MessageAdminHistoryExtraDto();
    }
}
