using System;
using IczpNet.Chat.SessionUnits;
using Xunit;

namespace IczpNet.Chat.SessionUnits;

public class MemberScore_Tests
{
    [Fact]
    public void Score_Hierarchy_And_Ordering_Should_Be_Strictly_Correct()
    {
        var baseTime = new DateTime(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc);
        var time1 = baseTime;
        var time2 = baseTime.AddMinutes(10);
        var time3 = baseTime.AddHours(1);

        // 1. 群主 (Owner / IsCreator)
        var owner = MemberScore.Create(isCreator: true, isAdmin: false, creationTime: time1);

        // 2. 两个管理员（早入群在前）
        var adminEarly = MemberScore.Create(isCreator: false, isAdmin: true, creationTime: time1);
        var adminLate = MemberScore.Create(isCreator: false, isAdmin: true, creationTime: time2);

        // 3. 两个普通成员（早入群在前）
        var memberEarly = MemberScore.Create(isCreator: false, isAdmin: false, creationTime: time1);
        var memberLate = MemberScore.Create(isCreator: false, isAdmin: false, creationTime: time3);

        // 验证身份反解
        Assert.True(owner.IsCreator);
        Assert.False(owner.IsAdmin);

        Assert.False(adminEarly.IsCreator);
        Assert.True(adminEarly.IsAdmin);
        Assert.False(adminLate.IsCreator);
        Assert.True(adminLate.IsAdmin);

        Assert.False(memberEarly.IsCreator);
        Assert.False(memberEarly.IsAdmin);
        Assert.False(memberLate.IsCreator);
        Assert.False(memberLate.IsAdmin);

        // 验证严格递减排序: 群主 > 早管理员 > 晚管理员 > 早成员 > 晚成员
        Assert.True(owner.Value > adminEarly.Value);
        Assert.True(adminEarly.Value > adminLate.Value);
        Assert.True(adminLate.Value > memberEarly.Value);
        Assert.True(memberEarly.Value > memberLate.Value);

        // 验证浮点安全范围 (小于 JS IEEE 754 MAX_SAFE_INTEGER: 9007199254740991)
        Assert.True(owner.Value < 9_007_199_254_740_991.0);
        Assert.True(owner.Value > 0);
    }

    [Fact]
    public void CreationTime_RoundTrip_Should_Preserve_Precision()
    {
        var expectedTime = new DateTime(2026, 11, 21, 10, 30, 45, 123, DateTimeKind.Utc);

        var owner = MemberScore.Create(isCreator: true, isAdmin: false, creationTime: expectedTime);
        var admin = MemberScore.Create(isCreator: false, isAdmin: true, creationTime: expectedTime);
        var member = MemberScore.Create(isCreator: false, isAdmin: false, creationTime: expectedTime);

        Assert.Equal(expectedTime, owner.CreationTime);
        Assert.Equal(expectedTime, admin.CreationTime);
        Assert.Equal(expectedTime, member.CreationTime);
    }
}
