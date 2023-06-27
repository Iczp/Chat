namespace IczpNet.Chat.SessionUnits
{
    public class UpdateStatsResult
    {
        public int UpdateRemindAllCount { get; set; }

        public int UpdateLastMessageIdCount { get; set; }

        public int UpdatePublicBadgeCount { get; set; }

        public int UpdateLastMessageIdAndPublicBadgeCount { get; set; }

        public override string ToString()
        {
            return $"UpdateStatsResult: {nameof(UpdateRemindAllCount)}={UpdateRemindAllCount}, {nameof(UpdateLastMessageIdCount)}={UpdateLastMessageIdCount}, {nameof(UpdatePublicBadgeCount)}={UpdatePublicBadgeCount}, {nameof(UpdateLastMessageIdAndPublicBadgeCount)}={UpdateLastMessageIdAndPublicBadgeCount}";
        }
    }
}
