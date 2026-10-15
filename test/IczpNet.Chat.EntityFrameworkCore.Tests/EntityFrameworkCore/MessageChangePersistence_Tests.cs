using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

public class MessageChangePersistence_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
    [Fact]
    public async Task Change_log_is_independent_of_message_rows_and_reads_by_change_version()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var session = Guid.NewGuid();
            var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), SessionId = session, OwnerId = 1 };
            var oldMessage = new Message(unit);
            typeof(Message).GetProperty(nameof(Message.Id))!.SetValue(oldMessage, 5L);
            var newerMessage = new Message(unit);
            typeof(Message).GetProperty(nameof(Message.Id))!.SetValue(newerMessage, 700L);
            var time = new DateTime(2026, 10, 14, 2, 0, 0, DateTimeKind.Utc);
            db.Set<MessageChange>().Add(new MessageChange(Guid.NewGuid(), newerMessage, 1, time));
            db.Set<MessageChange>().Add(new MessageChange(Guid.NewGuid(), oldMessage, 2, time));
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            var repository = ServiceProvider.GetRequiredService<IMessageChangeRepository>();
            var empty = await repository.GetCheckpointAsync(session);
            Assert.Equal(session, empty.Epoch);
            Assert.Equal(0, empty.Version);
            Assert.Equal(empty, (await repository.GetCheckpointsAsync([session]))[session]);
            var first = await repository.ReadChangesAsync(session, 0, 2, 1);
            Assert.Equal(700, Assert.Single(first).MessageId);
            var second = await repository.ReadChangesAsync(session, 1, 2, 1);
            Assert.Equal(5, Assert.Single(second).MessageId);
            Assert.Empty(await repository.ReadChangesAsync(Guid.NewGuid(), 0, 2, 10));
            // No FK requires the source message to survive physical deletion.
            Assert.DoesNotContain(db.Model.FindEntityType(typeof(MessageChange))!.GetForeignKeys(),
                key => key.PrincipalEntityType.ClrType == typeof(Message));
            Assert.Contains(db.Model.FindEntityType(typeof(MessageChange))!.GetIndexes(),
                index => index.IsUnique && index.Properties.Select(x => x.Name).SequenceEqual(new[] { "SessionId", "Version" }));
        });
    }

    [Fact]
    public async Task Checkpoint_and_message_revision_do_not_change_the_hot_message_schema()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var state = (SessionMessageChangeState)Activator.CreateInstance(typeof(SessionMessageChangeState), true)!;
            var id = Guid.NewGuid();
            var epoch = Guid.NewGuid();
            typeof(SessionMessageChangeState).GetProperty(nameof(state.Id))!.SetValue(state, id);
            typeof(SessionMessageChangeState).GetProperty(nameof(state.Epoch))!.SetValue(state, epoch);
            typeof(SessionMessageChangeState).GetProperty(nameof(state.Version))!.SetValue(state, 7L);
            db.Set<SessionMessageChangeState>().Add(state);
            await db.SaveChangesAsync();
            var head = await ServiceProvider.GetRequiredService<IMessageChangeRepository>().GetCheckpointAsync(id);
            Assert.Equal(epoch, head.Epoch);
            Assert.Equal(7, head.Version);
            Assert.Null(db.Model.FindEntityType(typeof(Message))!.FindProperty(nameof(Message.SyncVersion)));
            var message = new Message(new SessionUnitCacheItem { Id = Guid.NewGuid(), SessionId = id });
            typeof(Message).GetMethod("SetSyncVersion", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(message, [7L]);
            Assert.Equal(7L, message.ExtraProperties[nameof(Message.SyncVersion)]);
        });
    }
}
