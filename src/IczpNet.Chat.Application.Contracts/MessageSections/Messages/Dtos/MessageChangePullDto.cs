using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public record MessageChangePullDto(
    string Epoch,
    string HeadChangeId,
    string NextCursor,
    bool HasMore,
    bool ResetRequired,
    List<MessageChangePullItemDto> Items);
