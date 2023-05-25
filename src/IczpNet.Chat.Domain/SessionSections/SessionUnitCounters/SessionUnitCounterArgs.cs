using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionUnitCounters
{
    public class SessionUnitCounterArgs
    {
        public SessionUnitCounterArgs(Guid sessionId, DateTime creationTime)
        {
            SessionId = sessionId;
            MessageCreationTime = creationTime;
        }

        public Guid SessionId { get; protected set; }

        public bool IsPrivate { get; set; } = false;

        public Guid? IgnoreSessionUnitId { get; set; }

        public bool IsRemindAll { get; set; }

        public long LastMessageId { get; set; }

        public DateTime MessageCreationTime { get; protected set; }

        public List<Guid> RemindSessionUnitIdList { get; set; }

        public List<Guid> FollowingSessionUnitIdList { get; set; }

        public List<Guid> PrivateBadgeSessionUnitIdList { get; set; }

        public override string ToString()
        {
            return $"SessionId={SessionId},IsRemindAll={IsRemindAll},LastMessageId={LastMessageId}";
        }

    }
}
