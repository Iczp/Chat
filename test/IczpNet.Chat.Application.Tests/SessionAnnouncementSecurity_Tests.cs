using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionSections.Announcements;
using IczpNet.Chat.SessionSections.Announcements.Dtos;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NSubstitute;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

public class SessionAnnouncementSecurity_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
    private SessionAnnouncementAppService CreateService(params long[] ownedIds)
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        var user = Substitute.For<ICurrentUser>();
        var userId = Guid.NewGuid();
        user.Id.Returns(userId);
        var objects = Substitute.For<IChatObjectManager>();
        objects.GetIdListByUserIdAsync(userId).Returns(ownedIds.ToList());
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(user);
        lazy.LazyGetRequiredService<IChatObjectManager>().Returns(objects);
        return new SessionAnnouncementAppService(GetRequiredService<ISessionAnnouncementRepository>(),
            GetRequiredService<ISessionAnnouncementReceiptRepository>(), GetRequiredService<ISessionUnitRepository>(),
            Substitute.For<ISessionAnnouncementManager>(), Substitute.For<ISessionPermissionChecker>(), Substitute.For<IMessageManager>())
        { LazyServiceProvider = lazy };
    }

    // Use relational queries without starting message events or requiring Redis.
    private static async Task InsertAsync<T>(ChatDbContext context, Dictionary<string, object> values)
    {
        var entity = context.Model.FindEntityType(typeof(T))!;
        var store = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
        var columns = new List<string>();
        var parameters = new List<object>();
        foreach (var property in entity.GetProperties())
        {
            if (property.GetComputedColumnSql() != null) continue;
            if (!values.TryGetValue(property.Name, out var value))
            {
                if (property.IsNullable) continue;
                var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                value = type == typeof(string) ? "" : type == typeof(Guid) ? Guid.NewGuid() :
                    type == typeof(DateTime) ? DateTime.UtcNow : Activator.CreateInstance(type);
            }
            var converter = property.GetTypeMapping().Converter;
            if (converter != null) value = converter.ConvertToProvider(value);
            columns.Add("\"" + property.GetColumnName(store) + "\"");
            parameters.Add(value ?? DBNull.Value);
        }
        await context.Database.ExecuteSqlRawAsync("INSERT INTO \"" + entity.GetTableName() + "\" (" +
            string.Join(",", columns) + ") VALUES (" + string.Join(",", Enumerable.Range(0, parameters.Count).Select(i => "{" + i + "}")) + ")",
            parameters.ToArray());
    }

    private async Task<(Guid Session, Guid Unit, Guid Announcement)> SeedAsync(ChatDbContext context,
        long owner = 1, bool killed = false, ChatObjectTypeEnums type = ChatObjectTypeEnums.Room)
    {
        await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
        var session = Guid.NewGuid();
        var unit = Guid.NewGuid();
        var announcement = Guid.NewGuid();
        await InsertAsync<ChatObject>(context, new() { ["Id"] = owner, ["Name"] = "owner", ["IsEnabled"] = true,
            ["IsActive"] = true, ["ObjectType"] = ChatObjectTypeEnums.Personal });
        await InsertAsync<Session>(context, new() { ["Id"] = session,
            ["Channel"] = type == ChatObjectTypeEnums.Square ? Channels.SquareChannel : Channels.RoomChannel });
        await InsertAsync<SessionUnit>(context, new() { ["Id"] = unit, ["SessionId"] = session,
            ["OwnerId"] = owner, ["DestinationObjectType"] = type });
        await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = unit, ["SessionId"] = session,
            ["MemberName"] = "member", ["IsEnabled"] = true, ["IsKilled"] = killed });
        await InsertAsync<SessionAnnouncement>(context, new() { ["Id"] = announcement, ["SessionId"] = session,
            ["PublisherSessionUnitId"] = unit, ["Title"] = "title", ["Content"] = "body", ["Status"] = AnnouncementStatus.Published });
        return (session, unit, announcement);
    }

    [Fact]
    public async Task Foreign_membership_cannot_mutate_or_forge_receipts()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var data = await SeedAsync(context, owner: 2);
            var service = CreateService(1);
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.UpdateAsync(data.Announcement,
                new UpdateSessionAnnouncementInput { SessionUnitId = data.Unit, Title = "hack", Content = "hack" }));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.SetPinnedAsync(data.Announcement,
                new SetPinnedInput { SessionUnitId = data.Unit, IsPinned = true }));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.DeleteAsync(data.Announcement, data.Unit));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.ConfirmReceiptAsync(data.Announcement,
                new ConfirmSessionAnnouncementInput { SessionUnitId = data.Unit }));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.RecordViewAsync(data.Announcement,
                new RecordAnnouncementViewInput { SessionUnitId = data.Unit }));
        });
    }

    [Fact]
    public async Task Nonmembers_cannot_read_details_lists_pinned_or_receipts()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var data = await SeedAsync(context, owner: 2);
            var service = CreateService(1);
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.GetAsync(data.Announcement));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.GetPinnedAsync(data.Session));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.GetListAsync(new() { SessionId = data.Session }));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.GetReceiptsAsync(data.Announcement, new PagedAndSortedResultRequestDto()));
        });
    }

    [Fact]
    public async Task Owned_membership_in_another_session_cannot_create_or_update_target_announcement()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var source = await SeedAsync(context);
            var target = await SeedAsync(context, owner: 2);
            var service = CreateService(1);
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.CreateAsync(new()
                { SessionUnitId = source.Unit, SessionId = target.Session, Title = "hack", Content = "hack" }));
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.UpdateAsync(target.Announcement,
                new() { SessionUnitId = source.Unit, Title = "hack", Content = "hack" }));
        });
    }

    [Theory]
    [InlineData(true, ChatObjectTypeEnums.Room)]
    [InlineData(false, ChatObjectTypeEnums.Personal)]
    public async Task Removed_members_and_private_chats_are_rejected(bool killed, ChatObjectTypeEnums type)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var data = await SeedAsync(context, killed: killed, type: type);
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => CreateService(1).RecordViewAsync(data.Announcement,
                new() { SessionUnitId = data.Unit }));
        });
    }

    [Theory]
    [InlineData(ChatObjectTypeEnums.Room)]
    [InlineData(ChatObjectTypeEnums.Square)]
    public async Task Member_search_is_translatable_and_stat_excludes_removed_members(ChatObjectTypeEnums type)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var data = await SeedAsync(context, type: type);
            var removed = Guid.NewGuid();
            await InsertAsync<SessionUnit>(context, new() { ["Id"] = removed, ["SessionId"] = data.Session, ["OwnerId"] = 2L });
            await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = removed, ["SessionId"] = data.Session,
                ["IsEnabled"] = true, ["IsKilled"] = true });
            await InsertAsync<SessionAnnouncementReceipt>(context, new() { ["Id"] = 1L, ["AnnouncementId"] = data.Announcement,
                ["SessionUnitId"] = data.Unit, ["ChatObjectId"] = 1L, ["IsConfirmed"] = true });
            await InsertAsync<SessionAnnouncementReceipt>(context, new() { ["Id"] = 2L, ["AnnouncementId"] = data.Announcement,
                ["SessionUnitId"] = removed, ["ChatObjectId"] = 2L, ["IsConfirmed"] = true });
            var service = CreateService(1);
            var members = await service.GetMembersAsync(data.Announcement, new() { SessionUnitId = data.Unit, Keyword = "member" });
            Assert.Single(members.Items);
            var stat = await service.GetStatAsync(data.Announcement, data.Unit);
            Assert.Equal(1, stat.TotalMembersCount);
            Assert.Equal(1, stat.ConfirmedCount);
            Assert.Equal(0, stat.UnconfirmedCount);
            Assert.True((await service.GetAsync(data.Announcement)).IsConfirmedByCurrentUser);
            Assert.Single((await service.GetReceiptsAsync(data.Announcement, new())).Items);
        });
    }

    [Fact]
    public async Task Repeated_views_and_confirmations_persist_one_receipt_and_atomic_counts()
    {
        var data = await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            return await SeedAsync(context);
        });
        var distributedLock = Substitute.For<IAbpDistributedLock>();
        distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<IAbpDistributedLockHandle>());
        var manager = new SessionAnnouncementManager(GetRequiredService<ISessionAnnouncementRepository>(),
            GetRequiredService<ISessionAnnouncementReceiptRepository>(), distributedLock, GetRequiredService<IUnitOfWorkManager>());
        await manager.RecordViewAsync(data.Announcement, data.Unit, 1, "first");
        await manager.RecordViewAsync(data.Announcement, data.Unit, 1, "second");
        await manager.ConfirmReceiptAsync(data.Announcement, data.Unit, 1, "first");
        await manager.ConfirmReceiptAsync(data.Announcement, data.Unit, 1, "second");
        await WithUnitOfWorkAsync(async () =>
        {
            var announcement = await GetRequiredService<ISessionAnnouncementRepository>().GetAsync(data.Announcement);
            Assert.Equal(1, announcement.ReadCount);
            Assert.Equal(1, announcement.ConfirmedCount);
            var receipts = await GetRequiredService<ISessionAnnouncementReceiptRepository>().GetListAsync();
            Assert.Single(receipts);
            Assert.True(receipts[0].IsConfirmed);
            Assert.Equal("first", receipts[0].DeviceId);
        });
    }

    [Fact]
    public async Task Reviewer_with_approve_permission_can_review_own_submission()
    {
        var data = await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var seeded = await SeedAsync(context, owner: 1);
            await context.Database.ExecuteSqlRawAsync(
                "UPDATE \"ChatSessionAnnouncement\" SET \"Status\" = 3, \"Version\" = 0, \"IsApproved\" = 0, \"ConcurrencyStamp\" = {1} WHERE \"Id\" = {0}",
                seeded.Announcement,
                Guid.NewGuid().ToString("N"));
            return seeded;
        });

        await WithUnitOfWorkAsync(new AbpUnitOfWorkOptions { IsTransactional = false }, async () =>
        {
            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            var user = Substitute.For<ICurrentUser>();
            var userId = Guid.NewGuid();
            user.Id.Returns(userId);
            var objects = Substitute.For<IChatObjectManager>();
            objects.GetIdListByUserIdAsync(userId).Returns([1L]);
            lazy.LazyGetRequiredService<ICurrentUser>().Returns(user);
            lazy.LazyGetRequiredService<IChatObjectManager>().Returns(objects);

            var checker = Substitute.For<ISessionPermissionChecker>();
            checker.IsGrantedAsync(IczpNet.Chat.Permissions.SessionPermissionDefinitionConsts.AnnouncementPermission.Approve, Arg.Any<SessionUnit>())
                .Returns(true);

            var distributedLock = Substitute.For<IAbpDistributedLock>();
            distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(Substitute.For<IAbpDistributedLockHandle>());

            var manager = new SessionAnnouncementManager(
                GetRequiredService<ISessionAnnouncementRepository>(),
                GetRequiredService<ISessionAnnouncementReceiptRepository>(),
                distributedLock,
                GetRequiredService<IUnitOfWorkManager>());

            var service = new SessionAnnouncementAppService(
                GetRequiredService<ISessionAnnouncementRepository>(),
                GetRequiredService<ISessionAnnouncementReceiptRepository>(),
                GetRequiredService<ISessionUnitRepository>(),
                manager,
                checker,
                Substitute.For<IMessageManager>())
            { LazyServiceProvider = lazy };

            var reviewed = await service.ReviewAsync(data.Announcement, new ReviewSessionAnnouncementInput
            {
                SessionUnitId = data.Unit,
                Approved = true,
                Remark = "允许审核本人提审",
                SendMessageNotification = false
            });

            Assert.Equal(AnnouncementStatus.Published, reviewed.Status);
            Assert.True(reviewed.IsApproved);
            Assert.Equal(1, reviewed.Version);
            Assert.Equal(1L, reviewed.ReviewerChatObjectId);
            Assert.Equal("允许审核本人提审", reviewed.ReviewRemark);
        });
    }
}
