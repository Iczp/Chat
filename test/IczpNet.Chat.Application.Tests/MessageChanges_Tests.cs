using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Threading;
using Xunit;

namespace IczpNet.Chat;

public class MessageChanges_Tests
{
    [DisableConventionalRegistration]
    public class Probe : MessageAppService
    {
        public SessionUnit Unit { get; } = (SessionUnit)Activator.CreateInstance(typeof(SessionUnit), true)!;
        public IMessageChangeRepository Changes { get; } = Substitute.For<IMessageChangeRepository>();
        public IObjectMapper<ChatDomainModule> DomainMapper { get; } = Substitute.For<IObjectMapper<ChatDomainModule>>();
        public IMessageManager Messages => MessageManager;
        public List<MessageChangeEntry> Entries { get; } = [];
        public MessageChangeCheckpoint Head { get; set; } = new(Guid.NewGuid(), 0, 0);
        public Probe(bool enabled = true, bool enabledV2 = true) : base(Substitute.For<IDeletedRecorderManager>(), Substitute.For<IMessageRepository>(),
            Substitute.For<IReadedRecorderManager>(), Substitute.For<IOpenedRecorderManager>(), Substitute.For<IFavoritedRecorderManager>(),
            Substitute.For<IFollowManager>(), Substitute.For<ISessionUnitCacheManager>(), Substitute.For<ISessionUnitRepository>(),
            Substitute.For<ISessionUnitSettingManager>(), Substitute.For<ISessionUnitFriendshipMapper>(), Substitute.For<IMessageManager>())
        {
            typeof(SessionUnit).GetProperty(nameof(Unit.Id))!.SetValue(Unit, Guid.NewGuid());
            typeof(SessionUnit).GetProperty(nameof(Unit.SessionId))!.SetValue(Unit, Guid.NewGuid());
            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            LazyServiceProvider = lazy;
            lazy.LazyGetRequiredService<IOptions<MessageOptions>>().Returns(Microsoft.Extensions.Options.Options.Create(new MessageOptions { ChangeSyncEnabled = enabled, ChangeSyncV2Enabled = enabledV2 }));
            lazy.LazyGetRequiredService<IMessageChangeRepository>().Returns(Changes);
            lazy.LazyGetRequiredService<IObjectMapper<ChatDomainModule>>().Returns(DomainMapper);
            lazy.LazyGetRequiredService<IDataProtectionProvider>().Returns(new EphemeralDataProtectionProvider());
            lazy.LazyGetRequiredService<ICancellationTokenProvider>().Returns(Substitute.For<ICancellationTokenProvider>());
            Changes.GetCheckpointAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_ => Head);
            Changes.GetCheckpointsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(call => call.Arg<List<Guid>>().ToDictionary(id => id, _ => Head));
            Changes.ReadChangesAsync(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(call => Entries.Where(x => x.Version > call.ArgAt<long>(1) && x.Version <= call.ArgAt<long>(2)).Take(call.ArgAt<int>(3)).ToList());
            Changes.GetV2ScopesAsync(1, Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(call => call.Arg<List<Guid>>().Contains(Unit.Id)
                    ? [new(Unit.Id, Unit.SessionId.Value, 1, null)]
                    : []);
            var cache = Substitute.For<IMessageCacheManager>();
            cache.GetChangeWindowAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<int>(),
                Arg.Any<Func<Task<List<MessageChangeEntry>>>>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Func<Task<List<MessageChangeEntry>>>>()());
            lazy.LazyGetRequiredService<IMessageCacheManager>().Returns(cache);
            SessionUnitSettingManager.GetOrAddCacheAsync(Unit.Id).Returns(new SessionUnitSettingCacheItem { IsKilled = false });
            DeletedRecorderManager.GetRecorderMessageIdListAsync(Unit.Id, Arg.Any<List<long>>()).Returns([]);
        }
        protected override Task<SessionUnit> GetAndCheckPolicyAsync(string policyName, Guid id, bool checkIsKilled = true)
            => id == Unit.Id ? Task.FromResult(Unit) : throw new AbpAuthorizationException();
        protected override Task CheckPolicyForUserAsync(long? ownerId, Func<Task> func)
            => ownerId == 1 ? Task.CompletedTask : throw new AbpAuthorizationException();
        protected override MessageFastDto MapToMessage(MessageCacheItem item) => new()
        {
            Id = item.Id, Content = item.Content, ChangeSeq = item.ChangeSeq, IsRollbacked = item.IsRollbacked, RollbackTime = item.RollbackTime
        };
        public Task<List<MessageChangePageDto>> Query(string cursor = null, string snapshot = null) => QueryChangesAsync(new()
        { MaxResultCount = 1, Sessions = [new() { SessionUnitId = Unit.Id, Cursor = cursor, SnapshotCursor = snapshot }] });
    }

    [Fact]
    public async Task Disabled_sync_never_queries_undeployed_tables()
    {
        var service = new Probe(false);
        var error = await Assert.ThrowsAsync<BusinessException>(() => service.Query());
        Assert.Equal("Chat:MessageChangeSyncDisabled", error.Code);
        Assert.Empty(service.Changes.ReceivedCalls());
    }

    [Fact]
    public async Task Hidden_entries_advance_the_cursor_and_snapshot_excludes_newer_changes()
    {
        var service = new Probe();
        var baseline = Assert.Single(await service.Query());
        Assert.True(baseline.ResetRequired);
        service.Head = service.Head with { Version = 2 };
        service.Entries.AddRange([
            new() { MessageId = 100, Version = 1, IsPrivate = true, SenderSessionUnitId = Guid.NewGuid(), ReceiverSessionUnitId = Guid.NewGuid() },
            new() { MessageId = 5, Version = 2, ChangeType = MessageChangeKind.Rollback }]);
        var hidden = Assert.Single(await service.Query(baseline.NextCursor));
        Assert.Empty(hidden.Items);
        Assert.True(hidden.HasMore);
        service.Head = service.Head with { Version = 3 };
        service.Entries.Add(new() { MessageId = 1, Version = 3, ChangeType = MessageChangeKind.Rollback });
        var visible = Assert.Single(await service.Query(hidden.NextCursor, hidden.SnapshotCursor));
        Assert.Equal(5, Assert.Single(visible.Items).MessageId);
        Assert.False(visible.HasMore);
        var nextRound = Assert.Single(await service.Query(visible.NextCursor));
        Assert.Equal(1, Assert.Single(nextRound.Items).MessageId);
    }

    [Fact]
    public async Task Corrupt_rotated_and_expired_cursors_require_calibration()
    {
        var service = new Probe();
        Assert.True(Assert.Single(await service.Query("tampered")).ResetRequired);
        var cursor = Assert.Single(await service.Query()).NextCursor;
        service.Head = service.Head with { Epoch = Guid.NewGuid() };
        Assert.True(Assert.Single(await service.Query(cursor)).ResetRequired);
        cursor = Assert.Single(await service.Query()).NextCursor;
        service.Head = service.Head with { Version = 2, MinVersion = 1 };
        Assert.True(Assert.Single(await service.Query(cursor)).ResetRequired);
    }

    [Fact]
    public async Task Deletion_is_returned_only_to_its_member_even_when_the_message_is_already_filtered_out()
    {
        var service = new Probe();
        var baseline = Assert.Single(await service.Query());
        service.Head = service.Head with { Version = 2 };
        service.Entries.AddRange([
            new() { MessageId = 5, Version = 1, ChangeType = MessageChangeKind.Delete,
                IsPrivate = true, ReceiverSessionUnitId = service.Unit.Id },
            new() { MessageId = 5, Version = 2, ChangeType = MessageChangeKind.Delete,
                ReceiverSessionUnitId = Guid.NewGuid() }]);
        service.DeletedRecorderManager.GetRecorderMessageIdListAsync(service.Unit.Id, Arg.Any<List<long>>()).Returns([5]);
        service.SessionUnitSettingManager.GetOrAddCacheAsync(service.Unit.Id)
            .Returns(new SessionUnitSettingCacheItem { IsKilled = true, ClearTime = DateTime.UtcNow });
        var page = Assert.Single(await service.Query(baseline.NextCursor));
        Assert.True(Assert.Single(page.Items).IsDeleted);
        Assert.False(page.Items[0].IsRollbacked);
        var other = Assert.Single(await service.Query(page.NextCursor, page.SnapshotCursor));
        Assert.Empty(other.Items);
        Assert.False(other.HasMore);
    }

    [Fact]
    public async Task Unowned_scope_is_not_queried_or_disclosed()
    {
        var service = new Probe();
        var result = Assert.Single(await service.QueryChangesAsync(new() { Sessions = [new() { SessionUnitId = Guid.NewGuid() }] }));
        Assert.Equal("Unavailable", result.ErrorCode);
        Assert.Empty(service.Changes.ReceivedCalls());
    }

    [Fact]
    public async Task V2_heads_query_only_owned_units_and_use_distinct_epoch_and_decimal_sequences()
    {
        var service = new Probe();
        var unavailable = Guid.NewGuid();
        service.Head = service.Head with { Version = 9007199254740993L, MinVersion = 9007199254740992L };
        var result = await service.GetChangeHeadsAsync(new() { ChatObjectId = 1, SessionUnitIds = [service.Unit.Id, unavailable] });
        var own = result.Items[0];
        Assert.Equal(service.Unit.Id, own.SessionUnitId);
        Assert.NotEqual(service.Head.Epoch.ToString(), own.Epoch);
        Assert.Equal("9007199254740993", own.HeadSeq);
        Assert.Equal("0", own.MinSeq);
        Assert.Equal("Unavailable", result.Items[1].ErrorCode);
        Assert.Null(result.Items[1].HeadSeq);
        await service.Changes.Received(1).GetCheckpointsAsync(
            Arg.Is<List<Guid>>(ids => ids.SequenceEqual(new[] { service.Unit.SessionId.Value })), Arg.Any<CancellationToken>());
        await Assert.ThrowsAsync<AbpAuthorizationException>(() =>
            service.GetChangeHeadsAsync(new() { ChatObjectId = 2, SessionUnitIds = [service.Unit.Id] }));
    }

    [Fact]
    public async Task V2_pagination_advances_over_filtered_positions_and_rejects_an_older_epoch()
    {
        var service = new Probe();
        var head = Assert.Single((await service.GetChangeHeadsAsync(new() { ChatObjectId = 1, SessionUnitIds = [service.Unit.Id] })).Items);
        service.Head = service.Head with { Version = 6, MinVersion = 4 };
        service.Changes.ReadV2ChangesAsync(Arg.Any<MessageChangeUnitScope>(), 0, 6, 2, Arg.Any<CancellationToken>())
            .Returns([new(2, 9007199254740993L, MessageChangeKind.Rollback, null), new(5, null, MessageChangeKind.Clear, DateTime.UtcNow)]);
        var input = new MessageChangeIncrementalInput
        {
            ChatObjectId = 1, SessionUnitId = service.Unit.Id, ChangeSeq = "0", Epoch = head.Epoch, MaxResultCount = 1
        };
        var first = await service.GetIncrementalChangesAsync(input);
        Assert.False(first.ResetRequired);
        Assert.True(first.HasMore);
        Assert.Equal("2", first.NextSeq);
        Assert.Equal("9007199254740993", Assert.Single(first.Items).MessageId);

        service.Changes.ReadV2ChangesAsync(Arg.Any<MessageChangeUnitScope>(), 2, 6, 2, Arg.Any<CancellationToken>())
            .Returns([new(5, null, MessageChangeKind.Clear, DateTime.UtcNow)]);
        input.ChangeSeq = first.NextSeq;
        var second = await service.GetIncrementalChangesAsync(input);
        Assert.False(second.HasMore);
        Assert.Equal("6", second.NextSeq);
        Assert.Equal("Clear", Assert.Single(second.Items).ChangeType);

        input.ChangeSeq = second.NextSeq;
        service.Head = service.Head with { Version = 7 };
        service.Changes.ReadV2ChangesAsync(Arg.Any<MessageChangeUnitScope>(), 6, 7, 2, Arg.Any<CancellationToken>()).Returns([]);
        var empty = await service.GetIncrementalChangesAsync(input);
        Assert.Empty(empty.Items);
        Assert.Equal("7", empty.NextSeq);
        service.Head = service.Head with { Epoch = Guid.NewGuid() };
        var reset = await service.GetIncrementalChangesAsync(input);
        Assert.True(reset.ResetRequired);
        Assert.Equal(input.ChangeSeq, reset.NextSeq);
    }

    [Fact]
    public async Task V2_rejects_disabled_features_before_reading_the_schema()
    {
        var service = new Probe(enabledV2: false);
        await Assert.ThrowsAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(() =>
            service.GetChangeHeadsAsync(new() { ChatObjectId = 1, SessionUnitIds = [service.Unit.Id] }));
        Assert.Empty(service.Changes.ReceivedCalls());
    }

    [Fact]
    public void V2_by_ids_accepts_decimal_strings_without_rounding()
    {
        var body = JsonSerializer.Deserialize<MessageByIdsInput>(
            """{"chatObjectId":"9007199254740993","messageIds":["9007199254740995"]}""", new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(9007199254740993L, body.ChatObjectId);
        Assert.Equal(9007199254740995L, Assert.Single(body.MessageIds));
    }

    [Fact]
    public async Task V2_by_ids_never_returns_stale_cached_body_or_body_for_unavailable_states()
    {
        var service = new Probe();
        var sender = new SessionUnitCacheItem { Id = service.Unit.Id, SessionId = service.Unit.SessionId.Value, OwnerId = 1 };
        var updated = new Message(sender);
        var recalled = new Message(sender);
        var deleted = new Message(sender);
        typeof(Message).GetProperty(nameof(Message.Id)).SetValue(updated, 10L);
        typeof(Message).GetProperty(nameof(Message.Id)).SetValue(recalled, 12L);
        typeof(Message).GetProperty(nameof(Message.Id)).SetValue(deleted, 13L);
        typeof(Message).GetProperty(nameof(Message.ChangeSeq)).SetValue(updated, 9L);
        typeof(Message).GetProperty(nameof(Message.IsRollbacked)).SetValue(recalled, true);

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
        var result = await service.GetByIdsAsync(new() { ChatObjectId = 1, SessionUnitId = service.Unit.Id, MessageIds = [10, 12, 13, 14, 15] });

        Assert.Equal("Visible", result.Items[0].Status);
        Assert.Equal("current", result.Items[0].Message.Content);
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
