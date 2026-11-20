using System;
using System.Linq;
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
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using NSubstitute;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Timing;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

[DependsOn(typeof(MessageQueryReviewTestModule))]
public class CallCenterRedisConcurrencyTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        if (Environment.GetEnvironmentVariable("CHAT_REAL_REDIS_TEST") != "1") return;
        context.Services.Replace(ServiceDescriptor.Singleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect("localhost:6379")));
        context.Services.AddSingleton<IDistributedLockProvider>(provider =>
            new RedisDistributedSynchronizationProvider(provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase()));
    }
}

public class CallCenterRedisConcurrency_Tests : ChatTestBase<CallCenterRedisConcurrencyTestModule>
{
    [Fact]
    public async Task Competing_queue_drainers_respect_capacity_with_real_redis_lock()
    {
        if (Environment.GetEnvironmentVariable("CHAT_REAL_REDIS_TEST") != "1") return;
        long shopId = 0;
        var firstSession = Guid.NewGuid();
        var secondSession = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var types = ServiceProvider.GetRequiredService<IRepository<ChatObjectType, string>>();
            var objects = ServiceProvider.GetRequiredService<IRepository<ChatObject, long>>();
            var type = await types.FindAsync(ChatObjectTypeEnums.ShopKeeper.ToString())
                ?? await types.InsertAsync(new ChatObjectType(ChatObjectTypeEnums.ShopKeeper.ToString())
                { Name = "shop", ObjectType = ChatObjectTypeEnums.ShopKeeper }, autoSave: true);
            shopId = (await objects.InsertAsync(new ChatObject("shop", Guid.NewGuid().ToString("N"), type, null), autoSave: true)).Id;
            var config = new CallCenterConfiguration(Guid.NewGuid(), shopId);
            config.Configure(CallCenterDistributionStrategy.ShopKeeper, CallCenterOverflowStrategy.Queue, 1, 30, false);
            await ServiceProvider.GetRequiredService<IRepository<CallCenterConfiguration, Guid>>().InsertAsync(config, autoSave: true);
            var assignments = ServiceProvider.GetRequiredService<IRepository<CallCenterAssignment, Guid>>();
            var now = ServiceProvider.GetRequiredService<IClock>().Now;
            await assignments.InsertAsync(new CallCenterAssignment(Guid.NewGuid(), shopId, firstSession, 101, now), autoSave: true);
            await assignments.InsertAsync(new CallCenterAssignment(Guid.NewGuid(), shopId, secondSession, 102, now.AddSeconds(1)), autoSave: true);
        });
        var first = ServiceProvider.GetRequiredService<ICallCenterManager>();
        var second = ServiceProvider.GetRequiredService<ICallCenterManager>();
        await Task.WhenAll(first.DrainQueueAsync(shopId), second.DrainQueueAsync(shopId));
        await WithUnitOfWorkAsync(async () =>
        {
            var assignments = ServiceProvider.GetRequiredService<IRepository<CallCenterAssignment, Guid>>();
            Assert.Equal(CallCenterAssignmentStatus.Assigned,
                (await assignments.GetAsync(x => x.SessionId == firstSession)).Status);
            Assert.Equal(CallCenterAssignmentStatus.Queued,
                (await assignments.GetAsync(x => x.SessionId == secondSession)).Status);
        });
    }

    [Fact]
    public async Task Concurrent_transfers_keep_one_current_assignee_with_real_redis_lock()
    {
        if (Environment.GetEnvironmentVariable("CHAT_REAL_REDIS_TEST") != "1") return;
        Guid sourceUnitId = Guid.Empty, sessionId = Guid.Empty;
        long firstWaiterId = 0, secondWaiterId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var types = ServiceProvider.GetRequiredService<IRepository<ChatObjectType, string>>();
            var objects = ServiceProvider.GetRequiredService<IRepository<ChatObject, long>>();
            async Task<ChatObjectType> TypeAsync(ChatObjectTypeEnums type)
            {
                var id = type.ToString();
                return await types.FindAsync(id) ?? await types.InsertAsync(new ChatObjectType(id)
                { Name = id, ObjectType = type }, autoSave: true);
            }
            var customer = await objects.InsertAsync(new ChatObject("customer", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.Personal), null), autoSave: true);
            var shop = await objects.InsertAsync(new ChatObject("shop", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.ShopKeeper), null), autoSave: true);
            firstWaiterId = (await objects.InsertAsync(new ChatObject("waiter1", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.ShopWaiter), shop.Id), autoSave: true)).Id;
            secondWaiterId = (await objects.InsertAsync(new ChatObject("waiter2", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.ShopWaiter), shop.Id), autoSave: true)).Id;
            var session = await ServiceProvider.GetRequiredService<ISessionGenerator>().MakeAsync(customer, shop);
            sessionId = session.Id;
            sourceUnitId = session.UnitList.Single(x => x.OwnerId == shop.Id).Id;
        });
        CallCenterManager NewManager() => new(
            ServiceProvider.GetRequiredService<ISessionUnitManager>(),
            ServiceProvider.GetRequiredService<ISessionGenerator>(),
            ServiceProvider.GetRequiredService<ISessionUnitIdGenerator>(),
            ServiceProvider.GetRequiredService<IChatObjectManager>(),
            Substitute.For<IMessageSender>(), Substitute.For<IServiceStateManager>())
        {
            LazyServiceProvider = ServiceProvider.GetRequiredService<Volo.Abp.DependencyInjection.IAbpLazyServiceProvider>()
        };
        await Task.WhenAll(
            NewManager().TransferToAsync(sourceUnitId, firstWaiterId, false),
            NewManager().TransferToAsync(sourceUnitId, secondWaiterId, false));
        await WithUnitOfWorkAsync(async () =>
        {
            var assignments = await ServiceProvider.GetRequiredService<IRepository<CallCenterAssignment, Guid>>()
                .GetListAsync(x => x.SessionId == sessionId);
            var assignment = Assert.Single(assignments);
            Assert.Contains(assignment.AgentId, new long?[] { firstWaiterId, secondWaiterId });
        });
    }

    [Fact]
    public async Task Two_managers_compete_for_one_session_with_real_redis_lock()
    {
        if (Environment.GetEnvironmentVariable("CHAT_REAL_REDIS_TEST") != "1") return;
        Assert.IsNotType<NullAbpDistributedLock>(ServiceProvider.GetRequiredService<IAbpDistributedLock>());
        long customerId = 0, shopId = 0;
        await WithUnitOfWorkAsync(async () =>
        {
            var types = ServiceProvider.GetRequiredService<IRepository<ChatObjectType, string>>();
            var objects = ServiceProvider.GetRequiredService<IRepository<ChatObject, long>>();
            async Task<ChatObjectType> TypeAsync(ChatObjectTypeEnums type)
            {
                var id = type.ToString();
                return await types.FindAsync(id) ?? await types.InsertAsync(new ChatObjectType(id)
                { Name = id, ObjectType = type }, autoSave: true);
            }
            customerId = (await objects.InsertAsync(new ChatObject("customer", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.Personal), null), autoSave: true)).Id;
            shopId = (await objects.InsertAsync(new ChatObject("shop", Guid.NewGuid().ToString("N"),
                await TypeAsync(ChatObjectTypeEnums.ShopKeeper), null), autoSave: true)).Id;
            var config = new CallCenterConfiguration(Guid.NewGuid(), shopId);
            config.Configure(CallCenterDistributionStrategy.ShopKeeper, CallCenterOverflowStrategy.AllowOverCapacity, 10, 30, false);
            await ServiceProvider.GetRequiredService<IRepository<CallCenterConfiguration, Guid>>().InsertAsync(config, autoSave: true);
        });
        var sessionId = Guid.NewGuid();
        Message NewMessage(long id)
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
        var first = ServiceProvider.GetRequiredService<ICallCenterManager>();
        var second = ServiceProvider.GetRequiredService<ICallCenterManager>();
        await Task.WhenAll(first.ProcessMessageAsync(NewMessage(100)), second.ProcessMessageAsync(NewMessage(101)));
        await WithUnitOfWorkAsync(async () =>
        {
            var assignments = await ServiceProvider.GetRequiredService<IRepository<CallCenterAssignment, Guid>>()
                .GetListAsync(x => x.SessionId == sessionId);
            var assignment = Assert.Single(assignments);
            Assert.Equal(shopId, assignment.AgentId);
            Assert.Equal(101, assignment.LastActivityMessageId);
        });
    }
}
