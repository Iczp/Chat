using System;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class UpdateStatsForSessionUnitArgs
    {
        public Guid SenderSessionUnitId { get; set; }

        public Guid? ReceiverSessionUnitId { get; set; }

        public long MessageId { get; set; }
    }
}
