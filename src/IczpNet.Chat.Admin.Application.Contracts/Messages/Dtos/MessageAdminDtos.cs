using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.Messages.Dtos;

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
