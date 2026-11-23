using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.CallCenters;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Xunit;
using System;
using Volo.Abp.Timing;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.ServiceStates;
using IczpNet.Chat.MessageSections;
using NSubstitute;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.EntityFrameworkCore;

public class CallCenterModel_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
    [Fact]
    public async Task Active_counts_exclude_expired_queued_and_other_shop_conversations()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var now = ServiceProvider.GetRequiredService<IClock>().Now;
            var config = new CallCenterConfiguration(Guid.NewGuid(), 101);
            config.Configure(CallCenterDistributionStrategy.LeastActive, CallCenterOverflowStrategy.Queue, 5, 30, true);
            db.CallCenterConfiguration.Add(config);

            CallCenterAssignment NewAssignment(long shop, long? agent, DateTime activity)
            {
                var item = new CallCenterAssignment(Guid.NewGuid(), shop, Guid.NewGuid(), 200, activity);
                if (agent.HasValue) item.Assign(agent.Value);
                return item;
            }

            db.CallCenterAssignment.AddRange(
                NewAssignment(101, 301, now.AddMinutes(-1)),
                NewAssignment(101, 301, now.AddMinutes(-2)),
                NewAssignment(101, 302, now.AddMinutes(-3)),
                NewAssignment(101, 301, now.AddMinutes(-31)),
                NewAssignment(101, null, now),
                NewAssignment(102, 301, now));
            await db.SaveChangesAsync();

            var manager = new CallCenterManager(Substitute.For<ISessionUnitManager>(), Substitute.For<ISessionGenerator>(),
                Substitute.For<ISessionUnitIdGenerator>(), Substitute.For<IChatObjectManager>(),
                Substitute.For<IMessageSender>(), Substitute.For<IServiceStateManager>())
            {
                LazyServiceProvider = ServiceProvider.GetRequiredService<IAbpLazyServiceProvider>()
            };
            var counts = await manager.GetActiveCountsAsync(101, [301, 302]);
            Assert.Equal(2, counts.Count);
            Assert.Equal(2, counts[301]);
            Assert.Equal(1, counts[302]);
        });
    }

    [Fact]
    public async Task Configuration_and_assignment_have_unique_and_queue_indexes()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var config = db.Model.FindEntityType(typeof(CallCenterConfiguration));
            var assignment = db.Model.FindEntityType(typeof(CallCenterAssignment));
            Assert.NotNull(config);
            Assert.NotNull(assignment);
            Assert.Contains(config.GetIndexes(), x => x.IsUnique && x.Properties.Select(p => p.Name).SequenceEqual([nameof(CallCenterConfiguration.ShopKeeperId)]));
            Assert.Contains(assignment.GetIndexes(), x => x.IsUnique && x.Properties.Select(p => p.Name).SequenceEqual([nameof(CallCenterAssignment.SessionId)]));
            Assert.Contains(assignment.GetIndexes(), x => x.Properties.Select(p => p.Name).SequenceEqual(
                [nameof(CallCenterAssignment.ShopKeeperId), nameof(CallCenterAssignment.Status), nameof(CallCenterAssignment.QueuedAt), nameof(CallCenterAssignment.Id)]));
        });
    }
}
