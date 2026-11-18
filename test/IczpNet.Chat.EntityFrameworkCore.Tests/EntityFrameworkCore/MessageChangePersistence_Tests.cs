using System;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

[Collection("MessageQueryReview")]
public class MessageChangePersistence_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
    [Fact]
    public async Task MessageChange_inherits_FullAuditedEntity_soft_deletes_previous_active_row_and_enforces_filtered_unique_index()
    {
        await WithUnitOfWorkAsync(new Volo.Abp.Uow.AbpUnitOfWorkOptions { IsTransactional = true }, async () =>
        {
            Assert.True(typeof(FullAuditedEntity<long>).IsAssignableFrom(typeof(MessageChange)));

            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var session = Guid.NewGuid();
            await MessageVisibilityQuery_Tests.InsertAsync<IczpNet.Chat.Sessions.Session>(db, new() { ["Id"] = session });
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
            {
                ["Id"] = 5001L,
                ["SessionId"] = session,
                ["SessionSeq"] = 1L,
                ["ExtraProperties"] = new Volo.Abp.Data.ExtraPropertyDictionary()
            });
            var message = await db.Message.SingleAsync(x => x.Id == 5001L);
            var repository = ServiceProvider.GetRequiredService<IMessageChangeRepository>();
            var dataFilter = ServiceProvider.GetRequiredService<IDataFilter>();

            var time = new DateTime(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);
            var firstEntry = await repository.AppendUpdateAsync(message, time);
            var secondEntry = await repository.AppendRollbackAsync(message, time.AddSeconds(5));

            Assert.True(firstEntry.Version > 0);
            Assert.True(secondEntry.Version > firstEntry.Version);

            // Active query (ISoftDelete enabled) naturally returns only the single latest change for MessageId 5001.
            var activeForMessage = await db.Set<MessageChange>().AsNoTracking()
                .Where(x => x.MessageId == 5001L)
                .ToListAsync();
            var singleActive = Assert.Single(activeForMessage);
            Assert.Equal(secondEntry.Version, singleActive.Id);
            Assert.False(singleActive.IsDeleted);

            // Disabling ISoftDelete reveals the soft-deleted historical record.
            using (dataFilter.Disable<ISoftDelete>())
            {
                var allForMessage = await db.Set<MessageChange>().AsNoTracking()
                    .Where(x => x.MessageId == 5001L)
                    .OrderBy(x => x.Id)
                    .ToListAsync();
                Assert.Equal(2, allForMessage.Count);
                Assert.True(allForMessage[0].IsDeleted);
                Assert.NotNull(allForMessage[0].DeletionTime);
                Assert.False(allForMessage[1].IsDeleted);
            }

            // Filtered unique index [IsDeleted] = 0 rejects a second active record for the same MessageId.
            db.Set<MessageChange>().Add(new MessageChange(session, 5001L, MessageChangeKind.Update, time.AddSeconds(10)));
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            db.ChangeTracker.Clear();

            var entityType = db.Model.FindEntityType(typeof(MessageChange))!;
            Assert.Contains(entityType.GetIndexes(),
                index => index.IsUnique &&
                         index.Properties.Select(x => x.Name).SequenceEqual(new[] { "MessageId" }) &&
                         index.GetFilter() == "[IsDeleted] = 0");
            Assert.Contains(entityType.GetIndexes(),
                index => index.Properties.Select(x => x.Name).SequenceEqual(new[] { "SessionId", "Id" }));
            Assert.Contains(entityType.GetIndexes(),
                index => index.Properties.Select(x => x.Name).SequenceEqual(new[] { "IsDeleted", "DeletionTime" }));
        });
    }

    [Fact]
    public async Task MessageChangeCleanupWorker_hard_deletes_expired_soft_deleted_records_while_preserving_recent_and_active_ones()
    {
        await WithUnitOfWorkAsync(new Volo.Abp.Uow.AbpUnitOfWorkOptions { IsTransactional = true }, async () =>
        {
            var settingProvider = ServiceProvider.GetRequiredService<ISettingProvider>();
            var retentionDays = await settingProvider.GetAsync(ChatSettings.MessageChangeRetentionDays, defaultValue: 0);
            Assert.Equal(30, retentionDays);

            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var clock = ServiceProvider.GetRequiredService<IClock>();
            var session = Guid.NewGuid();

            await MessageVisibilityQuery_Tests.InsertAsync<IczpNet.Chat.Sessions.Session>(db, new() { ["Id"] = session });
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
            {
                ["Id"] = 7001L,
                ["SessionId"] = session,
                ["SessionSeq"] = 1L,
                ["ExtraProperties"] = new Volo.Abp.Data.ExtraPropertyDictionary()
            });
            var message = await db.Message.SingleAsync(x => x.Id == 7001L);
            var repository = ServiceProvider.GetRequiredService<IMessageChangeRepository>();

            // 1st change: 45 days ago -> will be soft-deleted when 2nd change arrives, and its DeletionTime set to 40 days ago (expired).
            var first = await repository.AppendUpdateAsync(message, clock.Now.AddDays(-45));
            // 2nd change: 5 days ago -> soft-deletes 1st change; will itself be soft-deleted when 3rd change arrives (recent, < 30 days).
            var second = await repository.AppendUpdateAsync(message, clock.Now.AddDays(-5));
            // 3rd change: now -> active (IsDeleted == false).
            var third = await repository.AppendRollbackAsync(message, clock.Now);

            var dataFilter = ServiceProvider.GetRequiredService<IDataFilter>();
            using (dataFilter.Disable<ISoftDelete>())
            {
                await db.Set<MessageChange>()
                    .Where(x => x.Id == first.Version)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.DeletionTime, clock.Now.AddDays(-40)));
                await db.Set<MessageChange>()
                    .Where(x => x.Id == second.Version)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.DeletionTime, clock.Now.AddDays(-5)));
            }
            db.ChangeTracker.Clear();
        });

        var deletedCount = await MessageChangeCleanupWorker.CleanupExpiredAsync(ServiceProvider, batchSize: 1);
        Assert.Equal(1, deletedCount);

        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var dataFilter = ServiceProvider.GetRequiredService<IDataFilter>();
            using (dataFilter.Disable<ISoftDelete>())
            {
                var remaining = await db.Set<MessageChange>().AsNoTracking()
                    .Where(x => x.MessageId == 7001L)
                    .OrderBy(x => x.Id)
                    .ToListAsync();
                Assert.Equal(2, remaining.Count);
                Assert.True(remaining[0].IsDeleted);
                Assert.False(remaining[1].IsDeleted);
            }
        });
    }

    [Fact]
    public async Task MessageChangeCursor_enforces_user_chatObject_device_uniqueness_and_monotonic_advance()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var userId = Guid.NewGuid();
            var repository = ServiceProvider.GetRequiredService<IMessageChangeRepository>();

            var cursor = await repository.UpsertCursorAsync(null, userId, 13L, "DEVICE_001", 120L, DateTime.UtcNow);
            Assert.Equal(120L, cursor.LastAppliedMessageChangeId);
            Assert.True(cursor.IsInitialized);

            var same = await repository.UpsertCursorAsync(null, userId, 13L, "DEVICE_001", 90L, DateTime.UtcNow);
            Assert.Equal(120L, same.LastAppliedMessageChangeId);

            var advanced = await repository.UpsertCursorAsync(null, userId, 13L, "DEVICE_001", 125L, DateTime.UtcNow);
            Assert.Equal(125L, advanced.LastAppliedMessageChangeId);

            var otherChatObject = await repository.UpsertCursorAsync(null, userId, 14L, "DEVICE_001", 40L, DateTime.UtcNow);
            Assert.Equal(40L, otherChatObject.LastAppliedMessageChangeId);

            Assert.Contains(db.Model.FindEntityType(typeof(MessageChangeCursor))!.GetIndexes(),
                index => index.IsUnique && index.Properties.Select(x => x.Name).SequenceEqual(new[] { "TenantId", "UserId", "ChatObjectId", "DeviceId" }));
        });
    }
}
