using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionUnitCounters
{
    [Serializable]
    public class SessionUnitCounterArgs
    {
        public Guid SessionId { get; set; }

        public bool IsPrivate { get; set; } = false;

        public Guid SenderSessionUnitId { get; set; }

        public bool IsRemindAll { get; set; }

        public long LastMessageId { get; set; }

        public DateTime MessageCreationTime { get; set; }

        public List<Guid> RemindSessionUnitIdList { get; set; }

        public List<Guid> FollowingSessionUnitIdList { get; set; }

        public List<Guid> PrivateBadgeSessionUnitIdList { get; set; }

        public override string ToString()
        {
            return $"SessionId={SessionId},IsRemindAll={IsRemindAll},LastMessageId={LastMessageId}";
        }
    }
}
