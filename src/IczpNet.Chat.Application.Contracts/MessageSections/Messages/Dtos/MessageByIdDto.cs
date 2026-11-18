using System;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public record MessageByIdDto(
    Guid SessionUnitId,
    string MessageId,
    string Status,
    string MessageChangeId,
    int? EntityVersion,
    MessageFastDto Message,
    DateTime? RollbackTime);
