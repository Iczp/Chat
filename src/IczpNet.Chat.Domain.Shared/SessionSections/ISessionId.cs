using System;

namespace IczpNet.Chat.SessionSections;

public interface ISessionId
{
    Guid? SessionId { get; }
}
