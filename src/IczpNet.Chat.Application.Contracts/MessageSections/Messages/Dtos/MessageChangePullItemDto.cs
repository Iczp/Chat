using System;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public record MessageChangePullItemDto(
    string MessageChangeId,
    Guid SessionId,
    Guid SessionUnitId,
    string MessageId,
    string ChangeType);
