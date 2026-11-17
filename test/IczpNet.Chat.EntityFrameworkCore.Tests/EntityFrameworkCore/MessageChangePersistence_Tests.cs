using System;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

[Collection("MessageQueryReview")]
public class MessageChangePersistence_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
    [Fact]
    public async Task MessageChange_uses_identity_id_and_Message_MessageChangeId_points_to_latest()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
            var session = Guid.NewGuid();
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new()
            {
                ["Id"] = 5001L,
                ["SessionId"] = session,
                ["SessionSeq"] = 1L,
                ["ExtraProperties"] = new Volo.Abp.Data.ExtraPropertyDictionary()
            });

            var time = new DateTime(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);
            var firstChange = new MessageChange(session, 5001L, MessageChangeKind.Update, time);
            var secondChange = new MessageChange(session, 5001L, MessageChangeKind.Rollback, time.AddSeconds(5));
            db.Set<MessageChange>().Add(firstChange);
            db.Set<MessageChange>().Add(secondChange);
            await db.SaveChangesAsync();

            Assert.True(firstChange.Id > 0);
            Assert.True(secondChange.Id > firstChange.Id);

            await db.Message.Where(x => x.Id == 5001L)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.MessageChangeId, secondChange.Id));
            db.ChangeTracker.Clear();

            var persisted = await db.Message.AsNoTracking().SingleAsync(x => x.Id == 5001L);
            Assert.Equal(secondChange.Id, persisted.MessageChangeId);

            // Message.MessageChangeId has FK to MessageChange.Id; MessageChange.MessageId has no FK to Message.
            Assert.Contains(db.Model.FindEntityType(typeof(Message))!.GetForeignKeys(),
                key => key.PrincipalEntityType.ClrType == typeof(MessageChange));
            Assert.DoesNotContain(db.Model.FindEntityType(typeof(MessageChange))!.GetForeignKeys(),
                key => key.PrincipalEntityType.ClrType == typeof(Message));
            Assert.Contains(db.Model.FindEntityType(typeof(MessageChange))!.GetIndexes(),
                index => index.Properties.Select(x => x.Name).SequenceEqual(new[] { "SessionId", "Id" }));
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

            // Regressing ACK must keep 120.
            var same = await repository.UpsertCursorAsync(null, userId, 13L, "DEVICE_001", 90L, DateTime.UtcNow);
            Assert.Equal(120L, same.LastAppliedMessageChangeId);

            // Advancing ACK must update to 125.
            var advanced = await repository.UpsertCursorAsync(null, userId, 13L, "DEVICE_001", 125L, DateTime.UtcNow);
            Assert.Equal(125L, advanced.LastAppliedMessageChangeId);

            // Different ChatObject on same device & user gets independent cursor.
            var otherChatObject = await repository.UpsertCursorAsync(null, userId, 14L, "DEVICE_001", 40L, DateTime.UtcNow);
            Assert.Equal(40L, otherChatObject.LastAppliedMessageChangeId);

            Assert.Contains(db.Model.FindEntityType(typeof(MessageChangeCursor))!.GetIndexes(),
                index => index.IsUnique && index.Properties.Select(x => x.Name).SequenceEqual(new[] { "TenantId", "UserId", "ChatObjectId", "DeviceId" }));
        });
    }
}
