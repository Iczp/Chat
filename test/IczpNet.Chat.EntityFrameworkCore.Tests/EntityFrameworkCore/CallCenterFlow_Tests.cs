using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.CallCenters;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.ServiceStates;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.ShopWaiters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Timing;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

[DependsOn(typeof(MessageQueryReviewTestModule))]
public class CallCenterFlowTestModule : Volo.Abp.Modularity.AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var distributedLock = Substitute.For<IAbpDistributedLock>();
        distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<IAbpDistributedLockHandle>());
        context.Services.Replace(ServiceDescriptor.Singleton(distributedLock));
    }
}

public class CallCenterFlow_Tests : ChatTestBase<CallCenterFlowTestModule>
{
    [Theory]
    [InlineData(CallCenterOverflowStrategy.Queue, CallCenterAssignmentStatus.Queued)]
    [InlineData(CallCenterOverflowStrategy.ShopKeeper, CallCenterAssignmentStatus.Assigned)]
    [InlineData(CallCenterOverflowStrategy.AllowOverCapacity, CallCenterAssignmentStatus.Assigned)]
    public async Task Offline_staff_follow_configured_overflow(
        CallCenterOverflowStrategy overflow, CallCenterAssignmentStatus expected)
    {
        long customerId = 0, shopId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var types = ServiceProvider.GetRequiredService<IRepository<ChatObjectType, string>>();
            var objects = ServiceProvider.GetRequiredService<IRepository<ChatObject, long>>();
            async Task<ChatObjectType> TypeAsync(ChatObjectTypeEnums type)
            {
                var id = type.ToString();
                return await types.FindAsync(id) ?? await types.InsertAsync(new ChatObjectType(id) { Name = id, ObjectType = type }, autoSave: true);
            }
            customerId = (await objects.InsertAsync(new ChatObject("customer", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.Personal), null), autoSave: true)).Id;
            shopId = (await objects.InsertAsync(new ChatObject("shop", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.ShopKeeper), null), autoSave: true)).Id;
            var config = new CallCenterConfiguration(Guid.NewGuid(), shopId);
            config.Configure(CallCenterDistributionStrategy.ShopKeeper, overflow, 1, 30, true);
            await ServiceProvider.GetRequiredService<IRepository<CallCenterConfiguration, Guid>>().InsertAsync(config, autoSave: true);
        });
        var sessionId = Guid.NewGuid();
        var message = new Message(new SessionUnitCacheItem
        {
            Id = Guid.NewGuid(), SessionId = sessionId, OwnerId = customerId,
            DestinationId = shopId, OwnerObjectType = ChatObjectTypeEnums.Personal,
            DestinationObjectType = ChatObjectTypeEnums.ShopKeeper
        }) { CreationTime = ServiceProvider.GetRequiredService<IClock>().Now };
        typeof(Message).GetProperty(nameof(Message.Id))!.SetValue(message, 100L);
        var manager = ServiceProvider.GetRequiredService<ICallCenterManager>();
        await manager.ProcessMessageAsync(message);
        var assignment = await manager.GetAssignmentAsync(sessionId);
        Assert.Equal(expected, assignment!.Status);
        Assert.Equal(expected == CallCenterAssignmentStatus.Queued ? null : shopId, assignment.AgentId);
    }

    [Fact]
    public async Task New_waiter_backfills_existing_customer_session_idempotently()
    {
        long shopId = 0, waiterId = 0;
        Guid sessionId = Guid.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            var types = ServiceProvider.GetRequiredService<IRepository<ChatObjectType, string>>();
            var objects = ServiceProvider.GetRequiredService<IRepository<ChatObject, long>>();
            async Task<ChatObjectType> TypeAsync(ChatObjectTypeEnums type)
            {
                var id = type.ToString();
                return await types.FindAsync(id) ?? await types.InsertAsync(new ChatObjectType(id) { Name = id, ObjectType = type }, autoSave: true);
            }
            var customer = await objects.InsertAsync(new ChatObject("customer", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.Personal), null), autoSave: true);
            var shop = await objects.InsertAsync(new ChatObject("shop", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.ShopKeeper), null), autoSave: true);
            shopId = shop.Id;
            sessionId = (await ServiceProvider.GetRequiredService<ISessionGenerator>().MakeAsync(customer, shop)).Id;
            waiterId = (await objects.InsertAsync(new ChatObject("waiter", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.ShopWaiter), shop.Id), autoSave: true)).Id;
        });
        var job = ServiceProvider.GetRequiredService<ShopWaiterSessionSyncJob>();
        var args = new ShopWaiterSessionSyncJobArgs { ShopKeeperId = shopId, WaiterId = waiterId };
        await job.ExecuteAsync(args);
        await job.ExecuteAsync(args);
        await WithUnitOfWorkAsync(async () =>
        {
            var units = await ServiceProvider.GetRequiredService<IRepository<SessionUnit, Guid>>()
                .GetListAsync(x => x.SessionId == sessionId);
            Assert.Equal(3, units.Count);
            Assert.Single(units, x => x.OwnerId == waiterId);
        });
    }

    [Fact]
    public async Task Transfer_persists_assignee_without_removing_shared_members()
    {
        Guid shopUnitId = Guid.Empty, sessionId = Guid.Empty;
        long shopId = 0, waiterId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var types = ServiceProvider.GetRequiredService<IRepository<ChatObjectType, string>>();
            var objects = ServiceProvider.GetRequiredService<IRepository<ChatObject, long>>();
            async Task<ChatObjectType> TypeAsync(ChatObjectTypeEnums type)
            {
                var id = type.ToString();
                return await types.FindAsync(id) ?? await types.InsertAsync(new ChatObjectType(id) { Name = id, ObjectType = type }, autoSave: true);
            }
            var customer = await objects.InsertAsync(new ChatObject("customer", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.Personal), null), autoSave: true);
            var shop = await objects.InsertAsync(new ChatObject("shop", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.ShopKeeper), null), autoSave: true);
            var waiter = await objects.InsertAsync(new ChatObject("waiter", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.ShopWaiter), shop.Id), autoSave: true);
            shopId = shop.Id;
            waiterId = waiter.Id;
            var session = await ServiceProvider.GetRequiredService<ISessionGenerator>().MakeAsync(customer, shop);
            sessionId = session.Id;
            shopUnitId = session.UnitList.Single(x => x.OwnerId == shopId).Id;
        });
        var manager = new CallCenterManager(ServiceProvider.GetRequiredService<ISessionUnitManager>(),
            ServiceProvider.GetRequiredService<ISessionGenerator>(), ServiceProvider.GetRequiredService<ISessionUnitIdGenerator>(),
            ServiceProvider.GetRequiredService<IChatObjectManager>(), Substitute.For<IMessageSender>(),
            Substitute.For<IServiceStateManager>())
        {
            LazyServiceProvider = ServiceProvider.GetRequiredService<Volo.Abp.DependencyInjection.IAbpLazyServiceProvider>()
        };
        await manager.TransferToAsync(shopUnitId, waiterId, false);
        await manager.TransferToAsync(shopUnitId, waiterId, false);
        await WithUnitOfWorkAsync(async () =>
        {
            var assignments = await ServiceProvider.GetRequiredService<IRepository<CallCenterAssignment, Guid>>()
                .GetListAsync(x => x.SessionId == sessionId);
            var assignment = Assert.Single(assignments);
            Assert.Equal(waiterId, assignment.AgentId);
            var units = await ServiceProvider.GetRequiredService<IRepository<SessionUnit, Guid>>()
                .GetListAsync(x => x.SessionId == sessionId);
            Assert.Contains(units, x => x.OwnerId == shopId);
            Assert.Contains(units, x => x.OwnerId == waiterId);
            Assert.Equal(3, units.Count);
        });
    }

    [Fact]
    public async Task Full_shop_queues_and_recovers_after_activity_timeout()
    {
        long customerId = 0, shopId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var types = ServiceProvider.GetRequiredService<IRepository<ChatObjectType, string>>();
            var objects = ServiceProvider.GetRequiredService<IRepository<ChatObject, long>>();
            async Task<ChatObjectType> TypeAsync(ChatObjectTypeEnums type)
            {
                var id = type.ToString();
                return await types.FindAsync(id) ?? await types.InsertAsync(new ChatObjectType(id) { Name = id, ObjectType = type }, autoSave: true);
            }
            customerId = (await objects.InsertAsync(new ChatObject("customer", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.Personal), null), autoSave: true)).Id;
            shopId = (await objects.InsertAsync(new ChatObject("shop", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.ShopKeeper), null), autoSave: true)).Id;
            var config = new CallCenterConfiguration(Guid.NewGuid(), shopId);
            config.Configure(CallCenterDistributionStrategy.ShopKeeper, CallCenterOverflowStrategy.Queue, 1, 1, false);
            await ServiceProvider.GetRequiredService<IRepository<CallCenterConfiguration, Guid>>().InsertAsync(config, autoSave: true);
        });
        var manager = ServiceProvider.GetRequiredService<ICallCenterManager>();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        Message NewMessage(Guid sessionId, long id)
        {
            var message = new Message(new SessionUnitCacheItem
            {
                Id = Guid.NewGuid(), SessionId = sessionId, OwnerId = customerId,
                DestinationId = shopId, OwnerObjectType = ChatObjectTypeEnums.Personal,
                DestinationObjectType = ChatObjectTypeEnums.ShopKeeper
            }) { CreationTime = ServiceProvider.GetRequiredService<IClock>().Now };
            typeof(Message).GetProperty(nameof(Message.Id))!.SetValue(message, id);
            return message;
        }
        await manager.ProcessMessageAsync(NewMessage(first, 100));
        await manager.ProcessMessageAsync(NewMessage(second, 101));
        Assert.Equal(CallCenterAssignmentStatus.Queued, (await manager.GetAssignmentAsync(second))!.Status);
        await WithUnitOfWorkAsync(async () =>
        {
            var repository = ServiceProvider.GetRequiredService<IRepository<CallCenterAssignment, Guid>>();
            var firstAssignment = await repository.GetAsync(x => x.SessionId == first);
            typeof(CallCenterAssignment).GetProperty(nameof(CallCenterAssignment.LastActivityAt))!
                .SetValue(firstAssignment, ServiceProvider.GetRequiredService<IClock>().Now.AddMinutes(-2));
            await repository.UpdateAsync(firstAssignment, autoSave: true);
        });
        Assert.Equal(1, await manager.DrainQueueAsync(shopId));
        var recovered = await manager.GetAssignmentAsync(second);
        Assert.Equal(CallCenterAssignmentStatus.Assigned, recovered!.Status);
        Assert.Equal(shopId, recovered.AgentId);
        Assert.Equal(0, await manager.DrainQueueAsync(shopId));
    }

    [Fact]
    public async Task Customer_message_assigns_once_and_tracks_latest_activity()
    {
        long customerId = 0, shopId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var types = ServiceProvider.GetRequiredService<IRepository<ChatObjectType, string>>();
            var objects = ServiceProvider.GetRequiredService<IRepository<ChatObject, long>>();
            async Task<ChatObjectType> TypeAsync(ChatObjectTypeEnums type)
            {
                var id = type.ToString();
                return await types.FindAsync(id) ?? await types.InsertAsync(new ChatObjectType(id) { Name = id, ObjectType = type }, autoSave: true);
            }
            customerId = (await objects.InsertAsync(new ChatObject("customer", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.Personal), null), autoSave: true)).Id;
            shopId = (await objects.InsertAsync(new ChatObject("shop", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.ShopKeeper), null), autoSave: true)).Id;
            var configuration = new CallCenterConfiguration(Guid.NewGuid(), shopId);
            configuration.Configure(CallCenterDistributionStrategy.ShopKeeper, CallCenterOverflowStrategy.AllowOverCapacity, 1, 30, false);
            await ServiceProvider.GetRequiredService<IRepository<CallCenterConfiguration, Guid>>().InsertAsync(configuration, autoSave: true);
        });

        var manager = ServiceProvider.GetRequiredService<ICallCenterManager>();
        var sessionId = Guid.NewGuid();
        Message NewMessage(long id, DateTime time)
        {
            var message = new Message(new SessionUnitCacheItem
            {
                Id = Guid.NewGuid(), SessionId = sessionId, OwnerId = customerId,
                DestinationId = shopId, OwnerObjectType = ChatObjectTypeEnums.Personal,
                DestinationObjectType = ChatObjectTypeEnums.ShopKeeper
            }) { CreationTime = time };
            typeof(Message).GetProperty(nameof(Message.Id))!.SetValue(message, id);
            return message;
        }
        var now = ServiceProvider.GetRequiredService<IClock>().Now;
        await manager.ProcessMessageAsync(NewMessage(100, now));
        await manager.ProcessMessageAsync(NewMessage(101, now.AddSeconds(1)));
        await manager.ProcessMessageAsync(NewMessage(100, now));

        await WithUnitOfWorkAsync(async () =>
        {
            var repository = ServiceProvider.GetRequiredService<IRepository<CallCenterAssignment, Guid>>();
            var records = (await repository.GetListAsync()).Where(x => x.SessionId == sessionId).ToList();
            var assignment = Assert.Single(records);
            Assert.Equal(shopId, assignment.AgentId);
            Assert.Equal(CallCenterAssignmentStatus.Assigned, assignment.Status);
            Assert.Equal(101, assignment.LastActivityMessageId);
            Assert.Equal(now.AddSeconds(1), assignment.LastActivityAt);
        });
        await WithUnitOfWorkAsync(async () =>
        {
            var repository = ServiceProvider.GetRequiredService<IRepository<CallCenterAssignment, Guid>>();
            var assignment = await repository.GetAsync(x => x.SessionId == sessionId);
            typeof(CallCenterAssignment).GetProperty(nameof(CallCenterAssignment.LastActivityAt))!
                .SetValue(assignment, now.AddHours(-1));
            await repository.UpdateAsync(assignment, autoSave: true);
        });
        await manager.ProcessMessageAsync(NewMessage(101, now.AddSeconds(1)));
        Assert.Equal(now.AddHours(-1), (await manager.GetAssignmentAsync(sessionId))!.LastActivityAt);
    }
}
