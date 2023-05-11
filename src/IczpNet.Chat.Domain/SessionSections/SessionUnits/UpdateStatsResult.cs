namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class UpdateStatsResult
    {
        public int UpdateFollowingCount { get; set; }

        public int UpdateRemindAllCount { get; set; }

        public int UpdateLastMessageIdCount { get; set; }

        public int UpdateBadgeCount { get; set; }

        public override string ToString()
        {
            return $"UpdateStatsResult: {nameof(UpdateFollowingCount)}={UpdateFollowingCount}, {nameof(UpdateRemindAllCount)}={UpdateRemindAllCount}, {nameof(UpdateLastMessageIdCount)}={UpdateLastMessageIdCount}, {nameof(UpdateBadgeCount)}={UpdateBadgeCount}";
        }
    }
}
