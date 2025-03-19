using System;
using System.Collections.Generic;
using System.Linq;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public class SessionUnitCacheKey
{
    public string Type { get; private set; }

    public string Value { get; private set; }

    public SessionUnitCacheKey(Guid sessionId)
    {
        Type = "SessionId";
        Value = sessionId.ToString();
    }

    public SessionUnitCacheKey(long temporary)
    {
        Type = "Temporary";
        Value = temporary.ToString();
    }

    public SessionUnitCacheKey(IEnumerable<Guid> sessionUnitIdList)
    {
        if (sessionUnitIdList.Count() != 2)
        {
            throw new ArgumentException($"{nameof(sessionUnitIdList)}.Count must be 2");
        }
        Type = "SessionUintIdList";
        Value = sessionUnitIdList.OrderBy(x => x).JoinAsString("-");
    }

    public override string ToString()
    {
        return $"{Type}_{Value}";
    }
}
