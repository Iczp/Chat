using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.SessionUnits.Dtos;
using NSubstitute;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Security.Claims;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitDisposal_Tests
{
    private class FakePrincipalAccessor : ICurrentPrincipalAccessor
    {
        public ClaimsPrincipal Principal { get; }
        public FakePrincipalAccessor(Guid userId)
        {
            Principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(AbpClaimTypes.UserId, userId.ToString())
            ], "fake"));
        }
        public IDisposable Change(ClaimsPrincipal principal) => throw new NotSupportedException();
    }

    [Fact]
    public async Task RemoveAsync_Kills_SessionUnit_And_Returns_Remove_Disposal()
    {
        var unitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerId = 10001L;

        var unit = (SessionUnit)Activator.CreateInstance(typeof(SessionUnit), true)!;
        typeof(SessionUnit).GetProperty(nameof(unit.Id))!.SetValue(unit, unitId);
        typeof(SessionUnit).GetProperty(nameof(unit.OwnerId))!.SetValue(unit, ownerId);
        typeof(SessionUnit).GetProperty(nameof(unit.SessionId))!.SetValue(unit, Guid.NewGuid());
        typeof(SessionUnit).GetProperty(nameof(unit.DestinationId))!.SetValue(unit, 20002L);

        var disposalRecord = new SessionUnitDisposal(
            ownerId: ownerId,
            sessionUnitId: unitId,
            sessionId: unit.SessionId,
            destinationId: unit.DestinationId,
            disposalType: SessionUnitDisposalType.Remove,
            reason: SessionUnitDisposalReason.MemberQuit,
            operatorId: ownerId,
            description: "主动退出群聊");
        typeof(SessionUnitDisposal).GetProperty(nameof(disposalRecord.Id))!.SetValue(disposalRecord, 101L);

        var disposalRepo = Substitute.For<IRepository<SessionUnitDisposal, long>>();
        var disposalQueryable = new List<SessionUnitDisposal> { disposalRecord }.AsQueryable();
        disposalRepo.GetQueryableAsync().Returns(Task.FromResult(disposalQueryable));

        var appService = new SessionUnitDisposalAppService(disposalRepo);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        appService.LazyServiceProvider = lazy;

        var currentUser = new CurrentUser(new FakePrincipalAccessor(userId));
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(currentUser);

        var chatObjectManager = Substitute.For<IChatObjectManager>();
        chatObjectManager.GetIdListByUserIdAsync(userId).Returns(Task.FromResult(new List<long> { ownerId }));
        lazy.LazyGetRequiredService<IChatObjectManager>().Returns(chatObjectManager);

        var sessionUnitManager = Substitute.For<ISessionUnitManager>();
        sessionUnitManager.GetAsync(unitId).Returns(Task.FromResult(unit));
        lazy.LazyGetRequiredService<ISessionUnitManager>().Returns(sessionUnitManager);

        var settingManager = Substitute.For<ISessionUnitSettingManager>();
        lazy.LazyGetRequiredService<ISessionUnitSettingManager>().Returns(settingManager);

        var asyncExecuter = Substitute.For<IAsyncQueryableExecuter>();
        asyncExecuter.FirstOrDefaultAsync(Arg.Any<IQueryable<SessionUnitDisposal>>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(call => Task.FromResult(call.Arg<IQueryable<SessionUnitDisposal>>().FirstOrDefault()));
        lazy.LazyGetRequiredService<IAsyncQueryableExecuter>().Returns(asyncExecuter);

        var result = await appService.RemoveAsync(unitId, "主动退出");

        Assert.NotNull(result);
        Assert.Equal(SessionUnitDisposalType.Remove, result.DisposalType);
        Assert.Equal(unitId, result.SessionUnitId);
        await settingManager.Received(1).KillAsync(unitId, KillTypes.OwnerQuit);
    }

    [Fact]
    public async Task QuitAndDeleteAsync_Kills_And_SoftDeletes_SessionUnit()
    {
        var unitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ownerId = 10001L;

        var unit = (SessionUnit)Activator.CreateInstance(typeof(SessionUnit), true)!;
        typeof(SessionUnit).GetProperty(nameof(unit.Id))!.SetValue(unit, unitId);
        typeof(SessionUnit).GetProperty(nameof(unit.OwnerId))!.SetValue(unit, ownerId);
        typeof(SessionUnit).GetProperty(nameof(unit.SessionId))!.SetValue(unit, Guid.NewGuid());
        typeof(SessionUnit).GetProperty(nameof(unit.DestinationId))!.SetValue(unit, 20002L);

        var disposalRecord = new SessionUnitDisposal(
            ownerId: ownerId,
            sessionUnitId: unitId,
            sessionId: unit.SessionId,
            destinationId: unit.DestinationId,
            disposalType: SessionUnitDisposalType.Delete,
            reason: SessionUnitDisposalReason.UserDeletedSession,
            description: "退出并删除会话");
        typeof(SessionUnitDisposal).GetProperty(nameof(disposalRecord.Id))!.SetValue(disposalRecord, 102L);

        var disposalRepo = Substitute.For<IRepository<SessionUnitDisposal, long>>();
        var disposalQueryable = new List<SessionUnitDisposal> { disposalRecord }.AsQueryable();
        disposalRepo.GetQueryableAsync().Returns(Task.FromResult(disposalQueryable));

        var appService = new SessionUnitDisposalAppService(disposalRepo);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        appService.LazyServiceProvider = lazy;

        var currentUser = new CurrentUser(new FakePrincipalAccessor(userId));
        lazy.LazyGetRequiredService<ICurrentUser>().Returns(currentUser);

        var chatObjectManager = Substitute.For<IChatObjectManager>();
        chatObjectManager.GetIdListByUserIdAsync(userId).Returns(Task.FromResult(new List<long> { ownerId }));
        lazy.LazyGetRequiredService<IChatObjectManager>().Returns(chatObjectManager);

        var sessionUnitManager = Substitute.For<ISessionUnitManager>();
        sessionUnitManager.GetAsync(unitId).Returns(Task.FromResult(unit));
        lazy.LazyGetRequiredService<ISessionUnitManager>().Returns(sessionUnitManager);

        var settingManager = Substitute.For<ISessionUnitSettingManager>();
        lazy.LazyGetRequiredService<ISessionUnitSettingManager>().Returns(settingManager);

        var unitRepo = Substitute.For<ISessionUnitRepository>();
        lazy.LazyGetRequiredService<ISessionUnitRepository>().Returns(unitRepo);

        var disposalManager = Substitute.For<ISessionUnitDisposalManager>();
        disposalManager.RecordDisposalAsync(
            ownerId, unitId, unit.SessionId, unit.DestinationId,
            SessionUnitDisposalType.Delete, SessionUnitDisposalReason.UserDeletedSession,
            Arg.Any<long?>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<long?>())
            .Returns(Task.FromResult(disposalRecord));
        lazy.LazyGetRequiredService<ISessionUnitDisposalManager>().Returns(disposalManager);

        var result = await appService.QuitAndDeleteAsync(unitId);

        Assert.NotNull(result);
        Assert.Equal(SessionUnitDisposalType.Delete, result.DisposalType);
        await settingManager.Received(1).KillAsync(unitId, KillTypes.OwnerQuit);
        await unitRepo.Received(1).DeleteAsync(unit, autoSave: true);
        await disposalManager.Received(1).RecordDisposalAsync(
            ownerId, unitId, unit.SessionId, unit.DestinationId,
            SessionUnitDisposalType.Delete, SessionUnitDisposalReason.UserDeletedSession,
            description: "退出并删除会话");
    }

    [Fact]
    public async Task CreateIfNotContainsAsync_Restores_SoftDeleted_SessionUnit_With_Same_Id()
    {
        var unitId = Guid.NewGuid();
        var ownerId = 10001L;
        var destinationId = 10002L;

        var existingUnit = (SessionUnit)Activator.CreateInstance(typeof(SessionUnit), true)!;
        typeof(SessionUnit).GetProperty(nameof(existingUnit.Id))!.SetValue(existingUnit, unitId);
        typeof(SessionUnit).GetProperty(nameof(existingUnit.OwnerId))!.SetValue(existingUnit, ownerId);
        typeof(SessionUnit).GetProperty(nameof(existingUnit.DestinationId))!.SetValue(existingUnit, destinationId);
        existingUnit.IsDeleted = true;
        existingUnit.DeletionTime = DateTime.UtcNow.AddDays(-1);

        var newUnit = (SessionUnit)Activator.CreateInstance(typeof(SessionUnit), true)!;
        typeof(SessionUnit).GetProperty(nameof(newUnit.Id))!.SetValue(newUnit, Guid.NewGuid());
        typeof(SessionUnit).GetProperty(nameof(newUnit.OwnerId))!.SetValue(newUnit, ownerId);
        typeof(SessionUnit).GetProperty(nameof(newUnit.DestinationId))!.SetValue(newUnit, destinationId);

        var unitRepo = Substitute.For<ISessionUnitRepository>();
        unitRepo.FindAsync(Arg.Any<Expression<Func<SessionUnit, bool>>>())
            .Returns(Task.FromResult(existingUnit));

        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        var dataFilter = Substitute.For<IDataFilter>();
        var disposable = Substitute.For<IDisposable>();
        dataFilter.Disable<ISoftDelete>().Returns(disposable);
        lazy.LazyGetRequiredService<IDataFilter>().Returns(dataFilter);

        var disposalManager = Substitute.For<ISessionUnitDisposalManager>();
        lazy.LazyGetRequiredService<ISessionUnitDisposalManager>().Returns(disposalManager);

        var managerConstructor = typeof(SessionUnitManager).GetConstructors().Single();
        var managerArgs = managerConstructor.GetParameters().Select(p =>
        {
            if (p.ParameterType == typeof(ISessionUnitRepository)) return unitRepo;
            return Substitute.For([p.ParameterType], []);
        }).ToArray();

        var manager = (SessionUnitManager)managerConstructor.Invoke(managerArgs);
        manager.LazyServiceProvider = lazy;

        var result = await manager.CreateIfNotContainsAsync(newUnit);

        Assert.NotNull(result);
        Assert.Equal(unitId, result.Id); // EXACT SAME ID PRESERVED!
        Assert.False(result.IsDeleted);
        Assert.Null(result.DeletionTime);
        await unitRepo.Received(1).UpdateAsync(existingUnit, autoSave: true);
        await unitRepo.DidNotReceive().InsertAsync(newUnit, autoSave: true);
    }
}
