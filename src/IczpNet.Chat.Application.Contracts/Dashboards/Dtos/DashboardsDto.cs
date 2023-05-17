using System;

namespace IczpNet.Chat.Dashboards.Dtos
{
    public class DashboardsDto
    {
        public DateTime Now { get; set; }

        public long ChatObjectCount { get; set; }

        public long SessionCount { get; set; }

        public long SessionUnitCount { get; set; }

        public long MessageCount { get; set; }

        public long ReadedRecorderCount { get; set; }

        public long OpenedRecorderCount { get; set; }

        public long FollowCount { get; set; }

        public long SessionRequestCount { get; set; }
        
        public long SessionOrganizationCount { get; set; }

        public long SessionRoleCount { get; set; }

        public long SessionTagCount { get; set; }

        public long SessionUnitTagCount { get; set; }

        public long SessionUnitRoleCount { get; set; }

        public long SessionUnitOrganizationCount { get; set; }

        public long MessageReminderCount { get; set; }

        public long FavoriteCount { get; set; }
    }
}
