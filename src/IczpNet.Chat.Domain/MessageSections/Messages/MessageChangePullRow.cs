using System;

namespace IczpNet.Chat.MessageSections.Messages;

public record MessageChangePullRow(
    long MessageChangeId,
    Guid SessionId,
    Guid SessionUnitId,
    long MessageId,
    MessageChangeKind ChangeType,
    DateTime CreationTime);
