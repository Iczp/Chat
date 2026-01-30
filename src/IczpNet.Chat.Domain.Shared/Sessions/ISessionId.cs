using System;

namespace IczpNet.Chat.Sessions;

public interface ISessionId
{
    Guid? SessionId { get; }
}
