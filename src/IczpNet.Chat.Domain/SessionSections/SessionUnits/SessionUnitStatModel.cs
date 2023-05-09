using System;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class SessionUnitStatModel
    {
        public Guid Id { get; set; }

        public int PublicBadge { get; set; }

        public int PrivateBadge { get; set; }

        public int FollowingCount { get; set; }

        public int RemindAllCount { get; set; }

        public int RemindMeCount { get; set; }

    }
}
