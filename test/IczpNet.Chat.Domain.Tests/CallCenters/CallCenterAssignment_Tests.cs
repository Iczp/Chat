using System;
using IczpNet.Chat.CallCenters;
using Volo.Abp;
using Xunit;

namespace IczpNet.Chat;

public class CallCenterAssignment_Tests
{
    [Fact]
    public void Assigned_conversation_releases_capacity_at_timeout()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var assignment = new CallCenterAssignment(Guid.NewGuid(), 10, Guid.NewGuid(), 20, now);
        Assert.False(assignment.IsActive(now, TimeSpan.FromMinutes(30)));
        assignment.Assign(30);
        Assert.True(assignment.IsActive(now.AddMinutes(29), TimeSpan.FromMinutes(30)));
        Assert.False(assignment.IsActive(now.AddMinutes(30), TimeSpan.FromMinutes(30)));
        assignment.RecordActivity(1, now.AddMinutes(31));
        Assert.True(assignment.IsActive(now.AddMinutes(31), TimeSpan.FromMinutes(30)));
        assignment.Queue(now.AddMinutes(32));
        Assert.False(assignment.IsActive(now.AddMinutes(32), TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void Activity_does_not_go_backwards_when_events_arrive_out_of_order()
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var assignment = new CallCenterAssignment(Guid.NewGuid(), 10, Guid.NewGuid(), 20, now);
        assignment.Assign(30);
        assignment.RecordActivity(200, now.AddMinutes(2));
        assignment.RecordActivity(100, now.AddMinutes(1));
        Assert.Equal(200, assignment.LastActivityMessageId);
        Assert.Equal(now.AddMinutes(2), assignment.LastActivityAt);
        Assert.Equal(30, assignment.AgentId);
        Assert.Equal(CallCenterAssignmentStatus.Assigned, assignment.Status);
    }

    [Fact]
    public void Queue_resets_the_agent_and_preserves_the_new_queue_time()
    {
        var now = DateTime.UtcNow;
        var assignment = new CallCenterAssignment(Guid.NewGuid(), 10, Guid.NewGuid(), 20, now);
        assignment.Assign(30);
        assignment.Queue(now.AddMinutes(1));
        Assert.Null(assignment.AgentId);
        Assert.Equal(CallCenterAssignmentStatus.Queued, assignment.Status);
        Assert.Equal(now.AddMinutes(1), assignment.QueuedAt);
    }

    [Fact]
    public void Configuration_defaults_to_allow_over_capacity_and_rejects_invalid_values()
    {
        var config = new CallCenterConfiguration(Guid.NewGuid(), 10);
        Assert.Equal(CallCenterOverflowStrategy.AllowOverCapacity, config.OverflowStrategy);
        Assert.Throws<UserFriendlyException>(() => config.Configure(CallCenterDistributionStrategy.LeastActive,
            CallCenterOverflowStrategy.Queue, 0, 30, true));
        Assert.Throws<UserFriendlyException>(() => config.Configure((CallCenterDistributionStrategy)999,
            CallCenterOverflowStrategy.Queue, 1, 30, true));
    }
}
