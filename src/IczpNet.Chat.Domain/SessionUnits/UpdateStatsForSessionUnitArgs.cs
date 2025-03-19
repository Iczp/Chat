using System;

namespace IczpNet.Chat.SessionUnits;

public class UpdateStatsForSessionUnitArgs
{
    public Guid SenderSessionUnitId { get; set; }

    public long MessageId { get; set; }
}
