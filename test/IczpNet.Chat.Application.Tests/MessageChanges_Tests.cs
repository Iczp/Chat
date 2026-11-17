using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageServices;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.Options;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.Extensions.Options;
using NSubstitute;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Security.Claims;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

public class MessageChanges_Tests
{
    [DisableConventionalRegistration]
    public class Probe : MessageAppService
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public SessionUnit Unit { get; } = (SessionUnit)Activator.CreateInstance(typeof(SessionUnit), true)!;
        public IMessageChangeRepository Changes { get; } = Substitute.For<IMessageChangeRepository>();
        public IObjectMapper<ChatDomainModule> DomainMapper { get; } = Substitute.For<IObjectMapper<ChatDomainModule>>();
        public IMessageManager Messages => MessageManager;
        public MessageChangeWriteGateState Gate { get; set; } = new(MessageChangeWriteGate.InitialEpoch, 1, 0);
        public MessageChangeCursor Cursor { get; set; }

        public Probe(bool enabled = true, bool enabledV2 = true) : base(
            Substitute.For<IDeletedRecorderManager>(),
            Substitute.For<IMessageRepository>(),
            Substitute.For<IReadedRecorderManager>(),
            Substitute.For<IOpenedRecorderManager>(),
            Substitute.For<IFavoritedRecorderManager>(),
            Substitute.For<IFollowManager>(),
            Substitute.For<ISessionUnitCacheManager>(),
            Substitute.For<ISessionUnitRepository>(),
            Substitute.For<ISessionUnitSettingManager>(),
            Substitute.For<ISessionUnitFriendshipMapper>(),
            Substitute.For<IMessageManager>())
        {
            typeof(SessionUnit).GetProperty(nameof(Unit.Id))!.SetValue(Unit, Guid.NewGuid());
            typeof(SessionUnit).GetProperty(nameof(Unit.SessionId))!.SetValue(Unit, Guid.NewGuid());
            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            LazyServiceProvider = lazy;

            var currentUser = new CurrentUser(new FakePrincipalAccessor(UserId));
            var currentTenant = Substitute.For<ICurrentTenant>();
            var clock = Substitute.For<IClock>();
            clock.Now.Returns(new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc));

            lazy.LazyGetRequiredService<IOptions<MessageOptions>>().Returns(
                Microsoft.Extensions.Options.Options.Create(new MessageOptions { ChangeSyncEnabled = enabled, ChangeSyncV2Enabled = enabledV2 }));
            lazy.LazyGetRequiredService<IMessageChangeRepository>().Returns(Changes);
            lazy.LazyGetRequiredService<IObjectMapper<ChatDomainModule>>().Returns(DomainMapper);
            lazy.LazyGetRequiredService<ICancellationTokenProvider>().Returns(Substitute.For<ICancellationTokenProvider>());
            lazy.LazyGetRequiredService<ICurrentUser>().Returns(currentUser);
            lazy.LazyGetRequiredService<ICurrentTenant>().Returns(currentTenant);
            lazy.LazyGetRequiredService<IClock>().Returns(clock);

            Changes.GetWriteGateAsync(Arg.Any<CancellationToken>()).Returns(_ => Gate);
            Changes.GetCursorAsync(Arg.Any<Guid?>(), UserId, 1L, "DEVICE_001", Arg.Any<CancellationToken>())
                .Returns(_ => Cursor);
            Changes.UpsertCursorAsync(Arg.Any<Guid?>(), UserId, 1L, "DEVICE_001", Arg.Any<long>(), Arg.Any<DateTime>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    var ackId = call.ArgAt<long>(4);
                    var reset = call.ArgAt<bool>(6);
                    if (Cursor == null)
                    {
                        Cursor = new MessageChangeCursor(Guid.NewGuid(), null, UserId, 1L, "DEVICE_001", ackId, true, call.ArgAt<DateTime>(5));
                    }
                    else if (reset)
                    {
                        Cursor.Reset(ackId, call.ArgAt<DateTime>(5));
                    }
                    else
                    {
                        Cursor.Advance(ackId, call.ArgAt<DateTime>(5));
                    }
                    return Cursor;
                });

            Changes.GetV2ScopesAsync(1, Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(call => call.Arg<List<Guid>>().Contains(Unit.Id)
                    ? [new(Unit.Id, Unit.SessionId.Value, 1, null)]
                    : []);
        }

        protected override Task CheckPolicyForUserAsync(long? ownerId, Func<Task> func)
            => ownerId == 1 ? Task.CompletedTask : throw new AbpAuthorizationException();

        protected override MessageFastDto MapToMessage(MessageCacheItem item) => new()
        {
            Id = item.Id, Content = item.Content, ChangeSeq = item.ChangeSeq, IsRollbacked = item.IsRollbacked, RollbackTime = item.RollbackTime
        };

        private sealed class FakePrincipalAccessor(Guid userId) : ICurrentPrincipalAccessor
        {
            public ClaimsPrincipal Principal { get; } = new(new ClaimsIdentity([new Claim(AbpClaimTypes.UserId, userId.ToString())]));
            public IDisposable Change(ClaimsPrincipal principal) => throw new NotSupportedException();
        }
    }

    [Fact]
    public async Task V1_query_and_sync_items_are_disabled()
    {
        var service = new Probe();
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            service.QueryChangesAsync(new() { MaxResultCount = 10, Sessions = [new() { SessionUnitId = service.Unit.Id }] }));
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            service.SyncItemsAsync(new() { SessionUnitId = service.Unit.Id, MessageIds = [1] }));
    }

    [Fact]
    public async Task V2_head_returns_global_epoch_head_and_device_cursor()
    {
        var service = new Probe();
        service.Gate = new MessageChangeWriteGateState(Guid.NewGuid(), 5, 9007199254740993L);

        var initial = await service.GetChangeHeadAsync(new() { ChatObjectId = 1, DeviceId = "DEVICE_001" });
        Assert.False(initial.Initialized);
        Assert.Equal("0", initial.LastAppliedChangeId);
        Assert.Equal("9007199254740993", initial.HeadChangeId);
        Assert.Equal(64, initial.Epoch.Length);

        await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
            service.GetChangeHeadAsync(new() { ChatObjectId = 2, DeviceId = "DEVICE_001" }));
    }

    [Fact]
    public async Task V2_pull_paginates_advances_over_empty_ranges_and_detects_epoch_rotation()
    {
        var service = new Probe();
        service.Gate = new MessageChangeWriteGateState(Guid.NewGuid(), 10, 150);
        var head = await service.GetChangeHeadAsync(new() { ChatObjectId = 1, DeviceId = "DEVICE_001" });

        service.Changes.PullChangesAsync(1L, 100L, 150L, 1, Arg.Any<CancellationToken>())
            .Returns(new MessageChangePullResult(130L, true, [
                new(130L, service.Unit.SessionId.Value, service.Unit.Id, 9007199254740993L, MessageChangeKind.Rollback, DateTime.UtcNow)
            ]));

        var first = await service.PullChangesAsync(new()
        {
            ChatObjectId = 1,
            DeviceId = "DEVICE_001",
            Epoch = head.Epoch,
            AfterMessageChangeId = "100",
            MaxResultCount = 1
        });
        Assert.False(first.ResetRequired);
        Assert.True(first.HasMore);
        Assert.Equal("130", first.NextCursor);
        Assert.Equal("150", first.HeadChangeId);
        var item = Assert.Single(first.Items);
        Assert.Equal("Recalled", item.ChangeType);
        Assert.Equal("9007199254740993", item.MessageId);
        Assert.Equal(service.Unit.Id, item.SessionUnitId);

        // Empty visible page still advances cursor to 150.
        service.Changes.PullChangesAsync(1L, 130L, 150L, 10, Arg.Any<CancellationToken>())
            .Returns(new MessageChangePullResult(150L, false, []));
        var second = await service.PullChangesAsync(new()
        {
            ChatObjectId = 1,
            DeviceId = "DEVICE_001",
            Epoch = head.Epoch,
            AfterMessageChangeId = "130",
            MaxResultCount = 10
        });
        Assert.False(second.HasMore);
        Assert.Equal("150", second.NextCursor);
        Assert.Empty(second.Items);

        // Epoch rotation triggers ResetRequired.
        service.Gate = service.Gate with { Epoch = Guid.NewGuid() };
        var reset = await service.PullChangesAsync(new()
        {
            ChatObjectId = 1,
            DeviceId = "DEVICE_001",
            Epoch = head.Epoch,
            AfterMessageChangeId = "150",
            MaxResultCount = 10
        });
        Assert.True(reset.ResetRequired);
        Assert.Equal("150", reset.NextCursor);
    }

    [Fact]
    public async Task V2_ack_advances_monotonically_and_rejects_ahead_of_head()
    {
        var service = new Probe();
        service.Gate = new MessageChangeWriteGateState(Guid.NewGuid(), 10, 150);
        var head = await service.GetChangeHeadAsync(new() { ChatObjectId = 1, DeviceId = "DEVICE_001" });

        var ack125 = await service.AckChangesAsync(new()
        {
            ChatObjectId = 1,
            DeviceId = "DEVICE_001",
            Epoch = head.Epoch,
            LastAppliedMessageChangeId = "125"
        });
        Assert.True(ack125.Initialized);
        Assert.False(ack125.ResetRequired);
        Assert.Equal("125", ack125.LastAppliedChangeId);

        // Smaller ACK keeps 125.
        var ack110 = await service.AckChangesAsync(new()
        {
            ChatObjectId = 1,
            DeviceId = "DEVICE_001",
            Epoch = head.Epoch,
            LastAppliedMessageChangeId = "110"
        });
        Assert.Equal("125", ack110.LastAppliedChangeId);

        // ACK beyond HeadChangeId throws.
        await Assert.ThrowsAsync<UserFriendlyException>(() => service.AckChangesAsync(new()
        {
            ChatObjectId = 1,
            DeviceId = "DEVICE_001",
            Epoch = head.Epoch,
            LastAppliedMessageChangeId = "151"
        }));
    }

    [Fact]
    public async Task V2_rejects_disabled_features_before_reading_the_schema()
    {
        var service = new Probe(enabledV2: false);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            service.GetChangeHeadAsync(new() { ChatObjectId = 1, DeviceId = "DEVICE_001" }));
        Assert.Empty(service.Changes.ReceivedCalls());
    }

    [Fact]
    public void V2_by_ids_accepts_decimal_strings_without_rounding()
    {
        var body = JsonSerializer.Deserialize<MessageByIdsInput>(
            """{"chatObjectId":"9007199254740993","requests":[{"sessionUnitId":"11111111-1111-1111-1111-111111111111","messageIds":["9007199254740995"]}]}""",
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(9007199254740993L, body.ChatObjectId);
        Assert.Equal(9007199254740995L, Assert.Single(Assert.Single(body.Requests).MessageIds));
    }

    [Fact]
    public async Task V2_by_ids_never_returns_stale_cached_body_or_body_for_unavailable_states()
    {
        var service = new Probe();
        var sender = new SessionUnitCacheItem { Id = service.Unit.Id, SessionId = service.Unit.SessionId.Value, OwnerId = 1 };
        var updated = new Message(sender);
        var recalled = new Message(sender);
        var deleted = new Message(sender);
        typeof(Message).GetProperty(nameof(Message.Id))!.SetValue(updated, 10L);
        typeof(Message).GetProperty(nameof(Message.Id))!.SetValue(recalled, 12L);
        typeof(Message).GetProperty(nameof(Message.Id))!.SetValue(deleted, 13L);
        typeof(Message).GetProperty(nameof(Message.ChangeSeq))!.SetValue(updated, 9L);
        typeof(Message).GetProperty(nameof(Message.MessageChangeId))!.SetValue(updated, 209L);
        typeof(Message).GetProperty(nameof(Message.IsRollbacked))!.SetValue(recalled, true);

        service.Changes.ReadV2ItemsAsync(Arg.Any<MessageChangeUnitScope>(), Arg.Any<List<long>>(), Arg.Any<CancellationToken>())
            .Returns([new(updated, false, null), new(recalled, false, null), new(deleted, true, 11)]);
        service.Messages.GetManyCacheAsync(Arg.Any<IEnumerable<long>>(), true, false, Arg.Any<CancellationToken>())
            .Returns([new(new MessageCacheKey(10), new() { Id = 10, Content = "old", ChangeSeq = 7 })]);
        service.DomainMapper.Map<Message, MessageCacheItem>(updated).Returns(new MessageCacheItem
        {
            Id = 10, Content = "current", ChangeSeq = 9
        });

        service.Changes.ReadV2SoftDeletedIdsAsync(Arg.Any<MessageChangeUnitScope>(), Arg.Is<List<long>>(ids => ids.SequenceEqual(new long[] { 14, 15 })),
            Arg.Any<CancellationToken>()).Returns([14]);
        var result = await service.GetByIdsAsync(new()
        {
            ChatObjectId = 1,
            Requests = [new() { SessionUnitId = service.Unit.Id, MessageIds = [10, 12, 13, 14, 15] }]
        });

        Assert.Equal("Visible", result.Items[0].Status);
        Assert.Equal("current", result.Items[0].Message.Content);
        Assert.Equal("209", result.Items[0].MessageChangeId);
        Assert.Equal("9", result.Items[0].ChangeSeq);
        Assert.Equal("Recalled", result.Items[1].Status);
        Assert.Equal("DeletedForMe", result.Items[2].Status);
        Assert.Equal("11", result.Items[2].ChangeSeq);
        Assert.Equal("NotFound", result.Items[3].Status);
        Assert.Equal("NotAccessible", result.Items[4].Status);
        Assert.All(result.Items.Skip(1), x => Assert.Null(x.Message));
        await service.Messages.Received(1).GetManyCacheAsync(
            Arg.Is<IEnumerable<long>>(ids => ids.SequenceEqual(new long[] { 10 })), true, false, Arg.Any<CancellationToken>());
    }
}
