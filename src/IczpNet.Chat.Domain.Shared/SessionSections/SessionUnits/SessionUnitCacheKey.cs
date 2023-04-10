using System;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
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

        public override string ToString()
        {
            return $"{Type}_{Value}";
        }
    }
}
