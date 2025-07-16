using System;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public class SessionUnitCounterInfo
{
    public Guid Id { get; set; }

    public long? ReadedMessageId { get; set; }

    public int PublicBadge { get; set; }

    public int PrivateBadge { get; set; }

    public int FollowingCount { get; set; }

    public int RemindAllCount { get; set; }

    public int RemindMeCount { get; set; }

    public override string ToString()
    {
        return $"{nameof(SessionUnitCounterInfo)}: {nameof(Id)}={Id}, {nameof(ReadedMessageId)}={ReadedMessageId}, {nameof(PublicBadge)}={PublicBadge}, {nameof(PrivateBadge)}={PrivateBadge}, {nameof(FollowingCount)}={FollowingCount}, {nameof(RemindAllCount)}={RemindAllCount}, {nameof(RemindMeCount)}={RemindMeCount}";
    }

}
