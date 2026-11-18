using System;

namespace IczpNet.Chat.MessageSections.Messages;

public record MessageChangeUnitScope(Guid UnitId, Guid SessionId, long OwnerId, DateTime? ClearTime);
