using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<MessageChangeEntry> Entries { get; } = [];
        public MessageChangeCheckpoint Head { get; set; } = new(Guid.NewGuid(), 0, 0);
        public Probe(bool enabled = true) : base(Substitute.For<IDeletedRecorderManager>(), Substitute.For<IMessageRepository>(),
            Substitute.For<IReadedRecorderManager>(), Substitute.For<IOpenedRecorderManager>(), Substitute.For<IFavoritedRecorderManager>(),
            Substitute.For<IFollowManager>(), Substitute.For<ISessionUnitCacheManager>(), Substitute.For<ISessionUnitRepository>(),
            Substitute.For<ISessionUnitSettingManager>(), Substitute.For<ISessionUnitFriendshipMapper>(), Substitute.For<IMessageManager>())
        {
            typeof(SessionUnit).GetProperty(nameof(Unit.Id))!.SetValue(Unit, Guid.NewGuid());
            typeof(SessionUnit).GetProperty(nameof(Unit.SessionId))!.SetValue(Unit, Guid.NewGuid());
            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            LazyServiceProvider = lazy;
            lazy.LazyGetRequiredService<IOptions<MessageOptions>>().Returns(Microsoft.Extensions.Options.Options.Create(new MessageOptions { ChangeSyncEnabled = enabled }));
            lazy.LazyGetRequiredService<IMessageChangeRepository>().Returns(Changes);
            lazy.LazyGetRequiredService<IDataProtectionProvider>().Returns(new EphemeralDataProtectionProvider());
            lazy.LazyGetRequiredService<ICancellationTokenProvider>().Returns(Substitute.For<ICancellationTokenProvider>());
            Changes.GetCheckpointAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(_ => Head);
            Changes.GetCheckpointsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(call => call.Arg<List<Guid>>().ToDictionary(id => id, _ => Head));
            Changes.ReadChangesAsync(Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(call => Entries.Where(x => x.Version > call.ArgAt<long>(1) && x.Version <= call.ArgAt<long>(2)).Take(call.ArgAt<int>(3)).ToList());
            var cache = Substitute.For<IMessageCacheManager>();
            cache.GetChangeWindowAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<long>(), Arg.Any<int>(),
                Arg.Any<Func<Task<List<MessageChangeEntry>>>>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Func<Task<List<MessageChangeEntry>>>>()());
            lazy.LazyGetRequiredService<IMessageCacheManager>().Returns(cache);
            SessionUnitSettingManager.GetOrAddCacheAsync(Unit.Id).Returns(new SessionUnitSettingCacheItem { IsKilled = false });
            DeletedRecorderManager.GetRecorderMessageIdListAsync(Unit.Id, Arg.Any<List<long>>()).Returns([]);
        }
        protected override Task<SessionUnit> GetAndCheckPolicyAsync(string policyName, Guid id, bool checkIsKilled = true)
            => id == Unit.Id ? Task.FromResult(Unit) : throw new AbpAuthorizationException();
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
    public async Task Unowned_scope_is_not_queried_or_disclosed()
    {
        var service = new Probe();
        var result = Assert.Single(await service.QueryChangesAsync(new() { Sessions = [new() { SessionUnitId = Guid.NewGuid() }] }));
        Assert.Equal("Unavailable", result.ErrorCode);
        Assert.Empty(service.Changes.ReceivedCalls());
    }
}
