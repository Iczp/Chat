using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Xunit;
using static IczpNet.Chat.EntityFrameworkCore.MessageVisibilityQuery_Tests;

namespace IczpNet.Chat.EntityFrameworkCore;

[Collection("MessageQueryReview")]
public class MessageChangeV2Persistence_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
    [Fact]
    public async Task NotFound_is_reserved_for_soft_deleted_messages_the_member_could_otherwise_see()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
            var session = Guid.NewGuid();
            var otherSession = Guid.NewGuid();
            var unit = Guid.NewGuid();
            await InsertAsync<Session>(context, new() { ["Id"] = session });
            await InsertAsync<Session>(context, new() { ["Id"] = otherSession });
            await InsertAsync<SessionUnit>(context, new() { ["Id"] = unit, ["SessionId"] = session, ["OwnerId"] = 1L });
            await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = unit, ["SessionId"] = session });
            foreach (var (id, expectedSession, isPrivate) in new[]
            {
                (101L, session, false),
                (102L, session, true),
                (103L, otherSession, false)
            })
            {
                await InsertAsync<Message>(context, new()
                {
                    ["Id"] = id, ["SessionId"] = expectedSession, ["SessionSeq"] = id,
                    ["IsDeleted"] = true, ["IsPrivate"] = isPrivate, ["ExtraProperties"] = new ExtraPropertyDictionary()
                });
            }

            var repository = ServiceProvider.GetRequiredService<IMessageChangeRepository>();
            var scope = Assert.Single(await repository.GetScopesAsync(1, [unit]));
            Assert.Equal(101L, Assert.Single(await repository.ReadSoftDeletedIdsAsync(scope, [101, 102, 103, 104])));
            Assert.Empty(await repository.ReadItemsAsync(scope, [101, 102, 103, 104]));
            await context.SessionUnit.Where(x => x.Id == unit)
                .ExecuteUpdateAsync(row => row.SetProperty(x => x.IsDeleted, true));
            Assert.Empty(await repository.ReadSoftDeletedIdsAsync(scope, [101]));
        });
    }

    [Fact]
    public async Task PullChanges_returns_only_latest_effective_change_scoped_by_chatObject_and_advances_empty_ranges()
    {
        await WithUnitOfWorkAsync(new Volo.Abp.Uow.AbpUnitOfWorkOptions { IsTransactional = true }, async () =>
        {
            var context = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var sessionA = Guid.NewGuid();
            var sessionB = Guid.NewGuid();
            var unitA = Guid.NewGuid();
            var unitB = Guid.NewGuid();

            await InsertAsync<IczpNet.Chat.ChatObjects.ChatObject>(context, new() { ["Id"] = 10L, ["Name"] = "Owner10", ["ExtraProperties"] = new ExtraPropertyDictionary() });
            await InsertAsync<IczpNet.Chat.ChatObjects.ChatObject>(context, new() { ["Id"] = 20L, ["Name"] = "Owner20", ["ExtraProperties"] = new ExtraPropertyDictionary() });
            await InsertAsync<Session>(context, new() { ["Id"] = sessionA });
            await InsertAsync<Session>(context, new() { ["Id"] = sessionB });
            await InsertAsync<SessionUnit>(context, new() { ["Id"] = unitA, ["SessionId"] = sessionA, ["OwnerId"] = 10L });
            await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = unitA, ["SessionId"] = sessionA });
            await InsertAsync<SessionUnit>(context, new() { ["Id"] = unitB, ["SessionId"] = sessionB, ["OwnerId"] = 20L });
            await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = unitB, ["SessionId"] = sessionB });

            await InsertAsync<Message>(context, new() { ["Id"] = 5001L, ["SessionId"] = sessionA, ["SessionSeq"] = 1L, ["ExtraProperties"] = new ExtraPropertyDictionary() });
            await InsertAsync<Message>(context, new() { ["Id"] = 6001L, ["SessionId"] = sessionB, ["SessionSeq"] = 1L, ["ExtraProperties"] = new ExtraPropertyDictionary() });
            var msgA = await context.Message.SingleAsync(x => x.Id == 5001L);
            var msgB = await context.Message.SingleAsync(x => x.Id == 6001L);

            var repository = ServiceProvider.GetRequiredService<IMessageChangeRepository>();
            var now = new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc);

            // 5 consecutive edits on msgA -> 5 MessageChange rows, msgA.MessageChangeId points to latest.
            long latestMsgA = 0;
            for (var i = 0; i < 5; i++)
            {
                var entry = await repository.AppendUpdateAsync(msgA, now.AddSeconds(i));
                latestMsgA = entry.Version;
                await context.Message.Where(x => x.Id == 5001L)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.MessageChangeId, latestMsgA));
            }
            // Then rollback msgA.
            var recallA = await repository.AppendRollbackAsync(msgA, now.AddSeconds(10));
            await context.Message.Where(x => x.Id == 5001L)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.IsRollbacked, true)
                    .SetProperty(x => x.RollbackTime, recallA.ChangedAt)
                    .SetProperty(x => x.MessageChangeId, recallA.Version));

            // Other user's session change after recallA.
            var updateB = await repository.AppendUpdateAsync(msgB, now.AddSeconds(20));
            await context.Message.Where(x => x.Id == 6001L)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.MessageChangeId, updateB.Version));
            context.ChangeTracker.Clear();

            var headChangeId = await repository.GetHeadChangeIdAsync();
            Assert.Equal(updateB.Version, headChangeId);

            // Owner 10 pulls from 0: must get ONLY 1 item (recallA), and NextCursor advances past updateB to headChangeId.
            var pull10 = await repository.PullChangesAsync(10L, 0, headChangeId, 100);
            var item10 = Assert.Single(pull10.Items);
            Assert.Equal(recallA.Version, item10.MessageChangeId);
            Assert.Equal(5001L, item10.MessageId);
            Assert.Equal(MessageChangeKind.Rollback, item10.ChangeType);
            Assert.Equal(unitA, item10.SessionUnitId);
            Assert.Equal(headChangeId, pull10.NextCursor);
            Assert.False(pull10.HasMore);

            // Owner 10 pulling after recallA sees empty items and cursor advances to headChangeId.
            var pull10Empty = await repository.PullChangesAsync(10L, recallA.Version, headChangeId, 100);
            Assert.Empty(pull10Empty.Items);
            Assert.Equal(headChangeId, pull10Empty.NextCursor);
            Assert.False(pull10Empty.HasMore);
        });
    }
}
