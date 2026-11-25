using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using IczpNet.Chat.SessionPermissions;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NSubstitute;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

public class RoomManagementPolicies_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
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

    private RoomAppService CreateService(long ownerId, ISessionPermissionChecker checker = null)
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        var user = Substitute.For<ICurrentUser>();
        var userId = Guid.NewGuid();
        user.Id.Returns(userId);
        var objects = Substitute.For<IChatObjectManager>();
        objects.GetIdListByUserIdAsync(userId).Returns([ownerId]);
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(user);
        lazy.LazyGetRequiredService<IChatObjectManager>().Returns(objects);
        lazy.LazyGetRequiredService<ISessionUnitRepository>().Returns(GetRequiredService<ISessionUnitRepository>());
        lazy.LazyGetRequiredService<IChatObjectRepository>().Returns(GetRequiredService<IChatObjectRepository>());
        lazy.LazyGetRequiredService<Volo.Abp.Domain.Repositories.IRepository<Session, Guid>>()
            .Returns(GetRequiredService<Volo.Abp.Domain.Repositories.IRepository<Session, Guid>>());
        lazy.LazyGetRequiredService<Volo.Abp.Domain.Repositories.IRepository<SessionRequests.SessionRequest, Guid>>()
            .Returns(GetRequiredService<Volo.Abp.Domain.Repositories.IRepository<SessionRequests.SessionRequest, Guid>>());
        lazy.LazyGetRequiredService<Volo.Abp.Linq.IAsyncQueryableExecuter>()
            .Returns(GetRequiredService<Volo.Abp.Linq.IAsyncQueryableExecuter>());
        lazy.LazyGetRequiredService<Volo.Abp.Timing.IClock>()
            .Returns(GetRequiredService<Volo.Abp.Timing.IClock>());
        lazy.LazyGetRequiredService<SessionSections.Announcements.ISessionAnnouncementRepository>()
            .Returns(GetRequiredService<SessionSections.Announcements.ISessionAnnouncementRepository>());

        if (checker == null)
        {
            checker = Substitute.For<ISessionPermissionChecker>();
        }

        return new RoomAppService(
            Substitute.For<IRoomManager>(),
            Substitute.For<IChatObjectCategoryManager>(),
            checker)
        {
            LazyServiceProvider = lazy
        };
    }

    [Fact]
    public async Task Ordinary_member_cannot_enter_or_update_policies_while_creator_can()
    {
        var data = await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            var roomId = 5000L;
            var creatorOwnerId = 5001L;
            var memberOwnerId = 5002L;
            var sessionId = Guid.NewGuid();
            var creatorUnitId = Guid.NewGuid();
            var memberUnitId = Guid.NewGuid();

            await InsertAsync<ChatObject>(context, new()
            {
                ["Id"] = roomId,
                ["Name"] = "测试群聊",
                ["IsEnabled"] = true,
                ["IsActive"] = true,
                ["ObjectType"] = ChatObjectTypeEnums.Room,
                ["VerificationMethod"] = VerificationMethods.None
            });
            await InsertAsync<ChatObject>(context, new()
            {
                ["Id"] = creatorOwnerId,
                ["Name"] = "群主",
                ["IsEnabled"] = true,
                ["IsActive"] = true,
                ["ObjectType"] = ChatObjectTypeEnums.Personal
            });
            await InsertAsync<ChatObject>(context, new()
            {
                ["Id"] = memberOwnerId,
                ["Name"] = "普通成员",
                ["IsEnabled"] = true,
                ["IsActive"] = true,
                ["ObjectType"] = ChatObjectTypeEnums.Personal
            });
            await InsertAsync<Session>(context, new()
            {
                ["Id"] = sessionId,
                ["OwnerId"] = roomId,
                ["Channel"] = Channels.RoomChannel,
                ["IsEnabled"] = true,
                ["AllowJoinChannels"] = RoomJoinChannels.All
            });
            await InsertAsync<SessionUnit>(context, new()
            {
                ["Id"] = creatorUnitId,
                ["SessionId"] = sessionId,
                ["OwnerId"] = creatorOwnerId,
                ["DestinationId"] = roomId,
                ["DestinationObjectType"] = ChatObjectTypeEnums.Room
            });
            await InsertAsync<SessionUnitSetting>(context, new()
            {
                ["SessionUnitId"] = creatorUnitId,
                ["SessionId"] = sessionId,
                ["IsCreator"] = true,
                ["IsAdmin"] = false,
                ["IsEnabled"] = true,
                ["IsKilled"] = false
            });
            await InsertAsync<SessionUnit>(context, new()
            {
                ["Id"] = memberUnitId,
                ["SessionId"] = sessionId,
                ["OwnerId"] = memberOwnerId,
                ["DestinationId"] = roomId,
                ["DestinationObjectType"] = ChatObjectTypeEnums.Room
            });
            await InsertAsync<SessionUnitSetting>(context, new()
            {
                ["SessionUnitId"] = memberUnitId,
                ["SessionId"] = sessionId,
                ["IsCreator"] = false,
                ["IsAdmin"] = false,
                ["IsEnabled"] = true,
                ["IsKilled"] = false
            });
            return (sessionId, roomId, creatorOwnerId, creatorUnitId, memberOwnerId, memberUnitId);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var memberService = CreateService(data.memberOwnerId);
            var memberSettings = await memberService.GetManagementSettingsAsync(data.memberUnitId);
            Assert.False(memberSettings.CanEnterManagement);
            Assert.False(memberSettings.IsCreator);
            Assert.False(memberSettings.IsAdmin);

            await Assert.ThrowsAsync<AbpAuthorizationException>(() => memberService.UpdatePoliciesAsync(
                new UpdateRoomPoliciesInput
                {
                    SessionUnitId = data.memberUnitId,
                    IsAllMuted = true
                }));

            var creatorService = CreateService(data.creatorOwnerId);
            var updated = await creatorService.UpdatePoliciesAsync(new UpdateRoomPoliciesInput
            {
                SessionUnitId = data.creatorUnitId,
                VerificationMethod = VerificationMethods.Required,
                IsAllMuted = true,
                InvitePolicy = RoomInvitePolicies.AdminsOnly,
                AllowJoinChannels = RoomJoinChannels.Invitation | RoomJoinChannels.QrCode,
                Description = "新群简介"
            });

            Assert.True(updated.CanEnterManagement);
            Assert.True(updated.IsCreator);
            Assert.Equal(VerificationMethods.Required, updated.VerificationMethod);
            Assert.True(updated.IsAllMuted);
            Assert.Equal(RoomInvitePolicies.AdminsOnly, updated.InvitePolicy);
            Assert.Equal(RoomJoinChannels.Invitation | RoomJoinChannels.QrCode, updated.AllowJoinChannels);
            Assert.Equal("新群简介", updated.Description);
        });
    }
}
