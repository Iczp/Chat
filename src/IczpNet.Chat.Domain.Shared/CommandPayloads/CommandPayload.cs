using System;
using System.Collections.Generic;

namespace IczpNet.Chat.CommandPayloads;


public class CommandPayload : CommandPayload<object>
{

}

public class CommandPayload<T>
{

    public Guid? AppUserId { get; set; }

    public List<ScopeUnit> Scopes { get; set; } = [];

    public string Command { get; set; }

    public T Payload { get; set; }

    public class ScopeUnit
    {
        public long ChatObjectId { get; set; }

        public Guid? SessionUnitId { get; set; }
    }
}
