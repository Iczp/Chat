using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections.Messages;

public record MessageChangePullResult(long NextCursor, bool HasMore, List<MessageChangePullRow> Items);
