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
using IczpNet.Chat.SessionRoles;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnitRoles;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using StackExchange.Redis;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

[DependsOn(typeof(MessageQueryReviewTestModule))]
public class RoomManagementPoliciesTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var db = Substitute.For<IDatabase>();
        var mux = Substitute.For<IConnectionMultiplexer>();
        mux.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(db);
        context.Services.AddSingleton(mux);
    }
}

public class RoomManagementPolicies_Tests : ChatTestBase<RoomManagementPoliciesTestModule>
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
            GetRequiredService<IRoomManager>(),
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

    [Fact]
    public async Task ClaimCreatorAsync_Succeeds_Only_When_No_Creator_And_No_Admin()
    {
        var data = await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var sessionId = Guid.NewGuid();
            const long roomId = 71001;
            const long member1OwnerId = 71002;
            const long member2OwnerId = 71003;
            var member1UnitId = Guid.NewGuid();
            var member2UnitId = Guid.NewGuid();

            await InsertAsync<ChatObject>(context, new()
            {
                ["Id"] = roomId,
                ["Name"] = "无主群",
                ["ObjectType"] = ChatObjectTypeEnums.Room,
                ["VerificationMethod"] = VerificationMethods.None,
                ["IsEnabled"] = true
            });
            await InsertAsync<ChatObject>(context, new() { ["Id"] = member1OwnerId, ["Name"] = "成员甲", ["ObjectType"] = ChatObjectTypeEnums.Personal });
            await InsertAsync<ChatObject>(context, new() { ["Id"] = member2OwnerId, ["Name"] = "成员乙", ["ObjectType"] = ChatObjectTypeEnums.Personal });

            await InsertAsync<Session>(context, new()
            {
                ["Id"] = sessionId,
                ["OwnerId"] = roomId,
                ["IsEnabled"] = true,
                ["IsAllMuted"] = false,
                ["InvitePolicy"] = RoomInvitePolicies.AllMembers,
                ["AllowJoinChannels"] = RoomJoinChannels.All
            });

            await InsertAsync<SessionUnit>(context, new()
            {
                ["Id"] = member1UnitId,
                ["SessionId"] = sessionId,
                ["OwnerId"] = member1OwnerId,
                ["DestinationId"] = roomId,
                ["DestinationObjectType"] = ChatObjectTypeEnums.Room
            });
            await InsertAsync<SessionUnitSetting>(context, new()
            {
                ["SessionUnitId"] = member1UnitId,
                ["SessionId"] = sessionId,
                ["IsCreator"] = false,
                ["IsAdmin"] = false,
                ["IsEnabled"] = true,
                ["IsKilled"] = false
            });

            await InsertAsync<SessionUnit>(context, new()
            {
                ["Id"] = member2UnitId,
                ["SessionId"] = sessionId,
                ["OwnerId"] = member2OwnerId,
                ["DestinationId"] = roomId,
                ["DestinationObjectType"] = ChatObjectTypeEnums.Room
            });
            await InsertAsync<SessionUnitSetting>(context, new()
            {
                ["SessionUnitId"] = member2UnitId,
                ["SessionId"] = sessionId,
                ["IsCreator"] = false,
                ["IsAdmin"] = false,
                ["IsEnabled"] = true,
                ["IsKilled"] = false
            });

            return (sessionId, roomId, member1OwnerId, member1UnitId, member2OwnerId, member2UnitId);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var member1Service = CreateService(data.member1OwnerId);
            var settingsBefore = await member1Service.GetManagementSettingsAsync(data.member1UnitId);
            Assert.False(settingsBefore.HasCreator);
            Assert.False(settingsBefore.HasAdmin);
            Assert.True(settingsBefore.CanClaimCreator);

            var roomManager = GetRequiredService<RoomManager>();
            await roomManager.ClaimCreatorAsync(data.member1UnitId, isSendMessageToRoom: false);

            var settingsAfter = await member1Service.GetManagementSettingsAsync(data.member1UnitId);
            Assert.True(settingsAfter.IsCreator);
            Assert.True(settingsAfter.HasCreator);
            Assert.False(settingsAfter.CanClaimCreator);
            Assert.True(settingsAfter.CanEnterManagement);

            var member2Service = CreateService(data.member2OwnerId);
            var member2Settings = await member2Service.GetManagementSettingsAsync(data.member2UnitId);
            Assert.True(member2Settings.HasCreator);
            Assert.False(member2Settings.CanClaimCreator);

            await Assert.ThrowsAsync<Volo.Abp.UserFriendlyException>(
                () => roomManager.ClaimCreatorAsync(data.member2UnitId, isSendMessageToRoom: false));
        });
    }

    [Fact]
    public async Task ClaimCreatorAsync_Blocked_When_Admin_Exists()
    {
        var data = await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var sessionId = Guid.NewGuid();
            const long roomId = 72001;
            const long adminOwnerId = 72002;
            const long memberOwnerId = 72003;
            var adminUnitId = Guid.NewGuid();
            var memberUnitId = Guid.NewGuid();

            await InsertAsync<ChatObject>(context, new()
            {
                ["Id"] = roomId,
                ["Name"] = "有管理无群主群",
                ["ObjectType"] = ChatObjectTypeEnums.Room,
                ["VerificationMethod"] = VerificationMethods.None,
                ["IsEnabled"] = true
            });
            await InsertAsync<ChatObject>(context, new() { ["Id"] = adminOwnerId, ["Name"] = "管理员", ["ObjectType"] = ChatObjectTypeEnums.Personal });
            await InsertAsync<ChatObject>(context, new() { ["Id"] = memberOwnerId, ["Name"] = "普通成员", ["ObjectType"] = ChatObjectTypeEnums.Personal });

            await InsertAsync<Session>(context, new()
            {
                ["Id"] = sessionId,
                ["OwnerId"] = roomId,
                ["IsEnabled"] = true,
                ["InvitePolicy"] = RoomInvitePolicies.AllMembers,
                ["AllowJoinChannels"] = RoomJoinChannels.All
            });

            await InsertAsync<SessionUnit>(context, new()
            {
                ["Id"] = adminUnitId,
                ["SessionId"] = sessionId,
                ["OwnerId"] = adminOwnerId,
                ["DestinationId"] = roomId,
                ["DestinationObjectType"] = ChatObjectTypeEnums.Room
            });
            await InsertAsync<SessionUnitSetting>(context, new()
            {
                ["SessionUnitId"] = adminUnitId,
                ["SessionId"] = sessionId,
                ["IsCreator"] = false,
                ["IsEnabled"] = true,
                ["IsKilled"] = false
            });
            var adminRoleId = Guid.NewGuid();
            await InsertAsync<SessionRole>(context, new()
            {
                ["Id"] = adminRoleId,
                ["SessionId"] = sessionId,
                ["Name"] = "管理员",
                ["IsDefault"] = false,
                ["PermissionCount"] = 0
            });
            await InsertAsync<SessionUnitRole>(context, new()
            {
                ["SessionUnitId"] = adminUnitId,
                ["SessionRoleId"] = adminRoleId
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

            return (memberOwnerId, memberUnitId);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var memberService = CreateService(data.memberOwnerId);
            var settings = await memberService.GetManagementSettingsAsync(data.memberUnitId);
            Assert.False(settings.HasCreator);
            Assert.True(settings.HasAdmin);
            Assert.False(settings.CanClaimCreator);

            var roomManager = GetRequiredService<RoomManager>();
            await Assert.ThrowsAsync<Volo.Abp.UserFriendlyException>(
                () => roomManager.ClaimCreatorAsync(data.memberUnitId, isSendMessageToRoom: false));
        });
    }

    [Fact]
    public async Task TransferCreatorAsync_Revokes_Creator_And_IsStatic_And_Rejects_Invalid_Target()
    {
        var data = await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");

            var sessionId = Guid.NewGuid();
            var roomId = 9003L;
            var ownerA = 10031L;
            var ownerB = 10032L;
            var ownerKilled = 10033L;
            var unitA = Guid.NewGuid();
            var unitB = Guid.NewGuid();
            var unitKilled = Guid.NewGuid();

            await InsertAsync<ChatObject>(context, new() { ["Id"] = roomId, ["Name"] = "转让测试群", ["ObjectType"] = ChatObjectTypeEnums.Room, ["IsEnabled"] = true });
            await InsertAsync<ChatObject>(context, new() { ["Id"] = ownerA, ["Name"] = "原群主", ["ObjectType"] = ChatObjectTypeEnums.Personal });
            await InsertAsync<ChatObject>(context, new() { ["Id"] = ownerB, ["Name"] = "新群主", ["ObjectType"] = ChatObjectTypeEnums.Personal });
            await InsertAsync<ChatObject>(context, new() { ["Id"] = ownerKilled, ["Name"] = "已退成员", ["ObjectType"] = ChatObjectTypeEnums.Personal });
            await InsertAsync<Session>(context, new() { ["Id"] = sessionId, ["OwnerId"] = roomId, ["IsEnabled"] = true });

            await InsertAsync<SessionUnit>(context, new() { ["Id"] = unitA, ["SessionId"] = sessionId, ["OwnerId"] = ownerA, ["DestinationId"] = roomId, ["DestinationObjectType"] = ChatObjectTypeEnums.Room });
            await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = unitA, ["SessionId"] = sessionId, ["IsCreator"] = true, ["IsStatic"] = true, ["IsEnabled"] = true, ["IsKilled"] = false });

            await InsertAsync<SessionUnit>(context, new() { ["Id"] = unitB, ["SessionId"] = sessionId, ["OwnerId"] = ownerB, ["DestinationId"] = roomId, ["DestinationObjectType"] = ChatObjectTypeEnums.Room });
            await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = unitB, ["SessionId"] = sessionId, ["IsCreator"] = false, ["IsStatic"] = false, ["IsEnabled"] = true, ["IsKilled"] = false });

            await InsertAsync<SessionUnit>(context, new() { ["Id"] = unitKilled, ["SessionId"] = sessionId, ["OwnerId"] = ownerKilled, ["DestinationId"] = roomId, ["DestinationObjectType"] = ChatObjectTypeEnums.Room });
            await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = unitKilled, ["SessionId"] = sessionId, ["IsCreator"] = false, ["IsStatic"] = false, ["IsEnabled"] = true, ["IsKilled"] = true });

            return (ownerA, unitA, ownerB, unitB, unitKilled);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var roomManager = GetRequiredService<RoomManager>();
            await Assert.ThrowsAsync<Volo.Abp.UserFriendlyException>(
                () => roomManager.TransferCreatorAsync(data.unitA, data.unitKilled, isSendMessageToRoom: false));

            await roomManager.TransferCreatorAsync(data.unitA, data.unitB, isSendMessageToRoom: false);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var checker = GetRequiredService<ISessionPermissionChecker>();
            var unitRepo = GetRequiredService<ISessionUnitRepository>();
            var unitA = await unitRepo.GetAsync(data.unitA);
            var unitB = await unitRepo.GetAsync(data.unitB);

            Assert.False(unitA.Setting.IsCreator);
            Assert.False(unitA.Setting.IsStatic);
            Assert.False(await checker.IsGrantedAsync(SessionPermissionDefinitionConsts.ChatObjectPermission.UpdateName, unitA));
            Assert.True(unitB.Setting.IsCreator);
            Assert.True(await checker.IsGrantedAsync(SessionPermissionDefinitionConsts.ChatObjectPermission.UpdateName, unitB));

            var serviceA = CreateService(data.ownerA, checker);
            var settingsA = await serviceA.GetManagementSettingsAsync(data.unitA);
            Assert.False(settingsA.IsCreator);
            Assert.False(settingsA.CanSetAdmin);
            Assert.False(settingsA.CanTransferOrDissolve);
            Assert.False(settingsA.CanManageSettings);

            var serviceB = CreateService(data.ownerB, checker);
            var settingsB = await serviceB.GetManagementSettingsAsync(data.unitB);
            Assert.True(settingsB.IsCreator);
            Assert.True(settingsB.CanSetAdmin);
            Assert.True(settingsB.CanTransferOrDissolve);
        });
    }

    [Fact]
    public async Task ClaimCreatorAsync_Rejects_Dissolved_Room()
    {
        var memberUnitId = await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
            var sessionId = Guid.NewGuid();
            var roomId = 9004L;
            var ownerId = 10041L;
            var unitId = Guid.NewGuid();
            await InsertAsync<ChatObject>(context, new() { ["Id"] = roomId, ["Name"] = "已解散群", ["ObjectType"] = ChatObjectTypeEnums.Room, ["IsEnabled"] = true });
            await InsertAsync<ChatObject>(context, new() { ["Id"] = ownerId, ["Name"] = "成员", ["ObjectType"] = ChatObjectTypeEnums.Personal });
            await InsertAsync<Session>(context, new() { ["Id"] = sessionId, ["OwnerId"] = roomId, ["IsEnabled"] = false });
            await InsertAsync<SessionUnit>(context, new() { ["Id"] = unitId, ["SessionId"] = sessionId, ["OwnerId"] = ownerId, ["DestinationId"] = roomId, ["DestinationObjectType"] = ChatObjectTypeEnums.Room });
            await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = unitId, ["SessionId"] = sessionId, ["IsCreator"] = false, ["IsEnabled"] = true, ["IsKilled"] = false });
            return unitId;
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var roomManager = GetRequiredService<RoomManager>();
            await Assert.ThrowsAsync<Volo.Abp.UserFriendlyException>(
                () => roomManager.ClaimCreatorAsync(memberUnitId, isSendMessageToRoom: false));
        });
    }

    [Fact]
    public async Task UpdatePoliciesAsync_Enforces_Per_Field_Permission_Without_Partial_Write()
    {
        var data = await WithUnitOfWorkAsync(async () =>
        {
            var context = await GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
            var sessionId = Guid.NewGuid();
            var roomId = 9005L;
            var ownerId = 10051L;
            var unitId = Guid.NewGuid();
            await InsertAsync<ChatObject>(context, new() { ["Id"] = roomId, ["Name"] = "策略权限群", ["Description"] = "原简介", ["ObjectType"] = ChatObjectTypeEnums.Room, ["IsEnabled"] = true });
            await InsertAsync<ChatObject>(context, new() { ["Id"] = ownerId, ["Name"] = "仅改名成员", ["ObjectType"] = ChatObjectTypeEnums.Personal });
            await InsertAsync<Session>(context, new() { ["Id"] = sessionId, ["OwnerId"] = roomId, ["IsEnabled"] = true, ["IsAllMuted"] = false });
            await InsertAsync<SessionUnit>(context, new() { ["Id"] = unitId, ["SessionId"] = sessionId, ["OwnerId"] = ownerId, ["DestinationId"] = roomId, ["DestinationObjectType"] = ChatObjectTypeEnums.Room });
            await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = unitId, ["SessionId"] = sessionId, ["IsCreator"] = false, ["IsEnabled"] = true, ["IsKilled"] = false });
            return (roomId, ownerId, unitId);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var checker = Substitute.For<ISessionPermissionChecker>();
            checker.IsGrantedAsync(SessionPermissionDefinitionConsts.ChatObjectPermission.UpdateName, Arg.Any<SessionUnit>()).Returns(true);
            checker.CheckAsync(SessionPermissionDefinitionConsts.ChatObjectPermission.UpdateName, Arg.Any<SessionUnit>()).Returns(Task.CompletedTask);
            checker.CheckAsync(SessionPermissionDefinitionConsts.ChatObjectPermission.ToggleAllInputEnabled, Arg.Any<SessionUnit>())
                .Returns<Task>(_ => throw new AbpAuthorizationException("No permission:ToggleAllInputEnabled"));

            var service = CreateService(data.ownerId, checker);
            await Assert.ThrowsAsync<AbpAuthorizationException>(() => service.UpdatePoliciesAsync(new UpdateRoomPoliciesInput
            {
                SessionUnitId = data.unitId,
                Description = "不应部分写入的新简介",
                IsAllMuted = true
            }));

            var room = await GetRequiredService<IChatObjectRepository>().GetAsync(data.roomId);
            Assert.Equal("原简介", room.Description);

            var updated = await service.UpdatePoliciesAsync(new UpdateRoomPoliciesInput
            {
                SessionUnitId = data.unitId,
                Description = "合法修改后的简介"
            });
            Assert.Equal("合法修改后的简介", updated.Description);
        });
    }
}
