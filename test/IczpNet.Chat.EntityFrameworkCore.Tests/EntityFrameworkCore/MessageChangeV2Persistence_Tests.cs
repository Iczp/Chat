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

public class MessageChangeV2Persistence_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
    [Fact]
    public async Task Projection_pages_use_latest_positions_and_filter_shared_and_private_visibility()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
            var session = Guid.NewGuid();
            var first = Guid.NewGuid();
            var second = Guid.NewGuid();
            var clearTime = new DateTime(2026, 10, 7, 10, 0, 0, DateTimeKind.Utc);
            const long start = 9007199254740990;
            await InsertAsync<Session>(context, new() { ["Id"] = session });
            await InsertAsync<SessionUnit>(context, new() { ["Id"] = first, ["SessionId"] = session, ["OwnerId"] = 1L });
            await InsertAsync<SessionUnit>(context, new() { ["Id"] = second, ["SessionId"] = session, ["OwnerId"] = 2L });
            await InsertAsync<SessionUnitSetting>(context, new()
            {
                ["SessionUnitId"] = first, ["SessionId"] = session,
                ["ClearTime"] = clearTime, ["ClearChangeSeq"] = start + 3
            });
            await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = second, ["SessionId"] = session });
            foreach (var (id, sequence, creation, privateSender) in new[]
            {
                (101L, start + 2, clearTime, (Guid?)null),
                (102L, start + 5, clearTime.AddMinutes(1), (Guid?)null),
                (103L, start + 7, clearTime.AddMinutes(2), (Guid?)first),
                (104L, start + 6, clearTime.AddMinutes(3), (Guid?)null)
            })
            {
                var values = new Dictionary<string, object>
                {
                    ["Id"] = id, ["SessionId"] = session, ["SessionSeq"] = id, ["CreationTime"] = creation,
                    ["ChangeSeq"] = sequence, ["ExtraProperties"] = new ExtraPropertyDictionary(),
                    ["IsPrivate"] = privateSender.HasValue, ["IsRollbacked"] = id == 102
                };
                if (privateSender.HasValue) values["SenderSessionUnitId"] = privateSender.Value;
                await InsertAsync<Message>(context, values);
            }
            await InsertAsync<DeletedRecorder>(context, new()
            {
                ["SessionUnitId"] = first, ["MessageId"] = 104L, ["ChangeSeq"] = start + 8
            });

            var repository = ServiceProvider.GetRequiredService<IMessageChangeRepository>();
            var own = Assert.Single(await repository.GetV2ScopesAsync(1, [first, second]));
            var peer = Assert.Single(await repository.GetV2ScopesAsync(2, [first, second]));
            Assert.Equal(clearTime, own.ClearTime);
            Assert.Null(peer.ClearTime);

            var firstPage = await repository.ReadV2ChangesAsync(own, start, start + 8, 2);
            Assert.Equal(new[] { MessageChangeKind.Clear, MessageChangeKind.Rollback }, firstPage.Select(x => x.Kind));
            Assert.Equal(new[] { start + 3, start + 5 }, firstPage.Select(x => x.Version));
            Assert.Null(firstPage[0].MessageId);
            Assert.Equal(clearTime, firstPage[0].Time);

            for (var seq = start + 9; seq <= start + 18; seq++)
                await context.Message.Where(x => x.Id == 103).ExecuteUpdateAsync(row => row.SetProperty(x => x.ChangeSeq, seq));
            var secondPage = Assert.Single(await repository.ReadV2ChangesAsync(own, start + 5, start + 8, 2));
            Assert.Equal(MessageChangeKind.Delete, secondPage.Kind);
            Assert.Equal(104, secondPage.MessageId);
            var latestEdit = Assert.Single(await repository.ReadV2ChangesAsync(own, start + 8, start + 18, 2));
            Assert.Equal(103, latestEdit.MessageId);
            Assert.Equal(start + 18, latestEdit.Version);

            var peerPage = await repository.ReadV2ChangesAsync(peer, start, start + 18, 10);
            Assert.Equal(new long?[] { 101, 102, 104 }, peerPage.Select(x => x.MessageId));
            var states = await repository.ReadV2ItemsAsync(own, [101, 102, 103, 104, 999]);
            Assert.Equal(new long[] { 102, 103, 104 }, states.Select(x => x.Message.Id).Order());
            Assert.Equal(start + 8, Assert.Single(states, x => x.IsDeleted).DeleteChangeSeq);
            Assert.Equal(new long[] { 103, 102 }, (await repository.ReadV2WindowAsync(own, null, null, 10, descending: true)).Select(x => x.Id));
        });
    }

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
            var scope = Assert.Single(await repository.GetV2ScopesAsync(1, [unit]));
            Assert.Equal(101L, Assert.Single(await repository.ReadV2SoftDeletedIdsAsync(scope, [101, 102, 103, 104])));
            Assert.Empty(await repository.ReadV2ItemsAsync(scope, [101, 102, 103, 104]));
            await context.SessionUnit.Where(x => x.Id == unit)
                .ExecuteUpdateAsync(row => row.SetProperty(x => x.IsDeleted, true));
            Assert.Empty(await repository.ReadV2SoftDeletedIdsAsync(scope, [101]));
        });
    }
}
