using System;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class SessionUnitCounterInfo
    {
        public Guid Id { get; set; }

        public long? LastMessageId { get; set; }

        public int PublicBadge { get; set; }

        public int PrivateBadge { get; set; }

        public int FollowingCount { get; set; }

        public int RemindAllCount { get; set; }

        public int RemindMeCount { get; set; }

    }
}
