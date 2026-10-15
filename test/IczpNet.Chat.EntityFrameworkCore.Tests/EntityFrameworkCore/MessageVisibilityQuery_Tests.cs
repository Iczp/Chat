using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using IczpNet.Chat.Specifications;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.DeletedRecorders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

[DependsOn(typeof(ChatEntityFrameworkCoreTestModule))]
public class MessageQueryReviewTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
        => context.Services.Replace(ServiceDescriptor.Singleton(Substitute.For<IDataSeeder>()));
}

public class MessageVisibilityQuery_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
    private static async Task InsertAsync<T>(ChatDbContext context, Dictionary<string, object> values)
    {
        var entity = context.Model.FindEntityType(typeof(T));
        var table = entity.GetTableName();
        var store = Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier.Table(table, entity.GetSchema());
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
                    type == typeof(DateTime) ? new DateTime(2026, 10, 6) : Activator.CreateInstance(type);
            }
            var converter = property.GetTypeMapping().Converter;
            if (converter != null) value = converter.ConvertToProvider(value);
            columns.Add("\"" + property.GetColumnName(store) + "\"");
            parameters.Add(value ?? DBNull.Value);
        }
        var arguments = string.Join(",", Enumerable.Range(0, parameters.Count).Select(i => "{" + i + "}"));
        var command = "INSERT INTO \"" + table + "\" (" + string.Join(",", columns) + ") VALUES (" + arguments + ")";
        await context.Database.ExecuteSqlRawAsync(command, parameters.ToArray());
    }

    [Fact]
    public async Task Repository_pages_do_not_require_an_anchor_and_do_not_skip_adjacent_or_sparse_ids()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
            var sessionId = Guid.NewGuid();
            var unitId = Guid.NewGuid();
            await InsertAsync<Session>(context, new() { ["Id"] = sessionId });
            await InsertAsync<SessionUnit>(context, new() { ["Id"] = unitId, ["SessionId"] = sessionId, ["OwnerId"] = 1L });
            await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = unitId, ["SessionId"] = sessionId });
            foreach (var id in new long[] { 90, 101, 102, 103, 104, 110 })
                await InsertAsync<Message>(context, new() { ["Id"] = id, ["SessionId"] = sessionId, ["IsPrivate"] = false });
            var repository = ServiceProvider.GetRequiredService<IMessageRepository>();
            // Shared raw windows certify open intervals without assuming consecutive global Ids.
            var recent = await repository.GetReadWindowAsync(sessionId, 0, long.MaxValue, 2, true);
            Assert.Equal(new long[] { 104, 110 }, recent.Items.Select(x => x.Id));
            Assert.Equal(103, recent.MinMessageId);
            var forward = await repository.GetReadWindowAsync(sessionId, 100, long.MaxValue, 2, false);
            Assert.Equal(new long[] { 101, 102 }, forward.Items.Select(x => x.Id));
            Assert.Equal(103, forward.MaxMessageId);
            var tail = await repository.GetReadWindowAsync(sessionId, 102, long.MaxValue, 10, false);
            Assert.Equal(new long[] { 103, 104, 110 }, tail.Items.Select(x => x.Id));
            Assert.Equal(long.MaxValue, tail.MaxMessageId);
            Assert.Equal(new long[] { 103, 102 }, await repository.GetVisibleIdsAsync(unitId, null, 104, 2, true));
            Assert.Equal(new long[] { 101, 90 }, await repository.GetVisibleIdsAsync(unitId, null, 102, 2, true));
            Assert.Equal(new long[] { 101, 102, 103 }, await repository.GetVisibleIdsAsync(unitId, 100, null, 3, false));
            Assert.Equal(new long[] { 104, 110 }, await repository.GetVisibleIdsAsync(unitId, 103, null, 3, false));
            await InsertAsync<Message>(context, new() { ["Id"] = 111L, ["SessionId"] = sessionId, ["IsPrivate"] = true,
                ["SenderSessionUnitId"] = Guid.NewGuid(), ["ReceiverSessionUnitId"] = Guid.NewGuid() });
            Assert.False(await repository.CanReadAsync(unitId, 111));
            Assert.Equal(6, await repository.CountVisibleAsync(unitId, null, null));
        });
    }

    [Fact]
    public async Task Calibration_returns_explicit_member_deletions_without_exposing_private_messages_to_other_members()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var context = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
            var session = Guid.NewGuid();
            var first = Guid.NewGuid();
            var second = Guid.NewGuid();
            await InsertAsync<Session>(context, new() { ["Id"] = session });
            foreach (var member in new[] { (first, 1L), (second, 2L) })
            {
                await InsertAsync<SessionUnit>(context, new() { ["Id"] = member.Item1, ["SessionId"] = session, ["OwnerId"] = member.Item2 });
                await InsertAsync<SessionUnitSetting>(context, new() { ["SessionUnitId"] = member.Item1, ["SessionId"] = session });
            }
            await InsertAsync<Message>(context, new() { ["Id"] = 105L, ["SessionId"] = session, ["ExtraProperties"] = new ExtraPropertyDictionary() });
            await InsertAsync<Message>(context, new() { ["Id"] = 106L, ["SessionId"] = session, ["ExtraProperties"] = new ExtraPropertyDictionary(),
                ["IsPrivate"] = true, ["SenderSessionUnitId"] = first, ["ReceiverSessionUnitId"] = Guid.NewGuid() });
            await InsertAsync<DeletedRecorder>(context, new() { ["SessionUnitId"] = first, ["MessageId"] = 105L });
            await InsertAsync<SessionMessageChangeState>(context, new() { ["Id"] = session, ["Epoch"] = session, ["Version"] = 7L });
            var message = new Message(new SessionUnitCacheItem { Id = first, SessionId = session, OwnerId = 1 });
            typeof(Message).GetProperty(nameof(Message.Id))!.SetValue(message, 105L);
            context.Set<MessageChange>().Add(new MessageChange(Guid.NewGuid(), message, 7, DateTime.UtcNow, first));
            await context.SaveChangesAsync();
            var changes = ServiceProvider.GetRequiredService<IMessageChangeRepository>();
            var own = await changes.ReadSyncItemsAsync(first, [105, 106, 999]);
            Assert.True(own.Single(x => x.MessageId == 105).IsDeleted);
            Assert.Equal(7, own.Single(x => x.MessageId == 105).SyncVersion);
            var peer = await changes.ReadSyncItemsAsync(second, [105, 106, 999]);
            Assert.False(Assert.Single(peer).IsDeleted);
            Assert.Equal(105, peer[0].MessageId);
            Assert.Equal(0, peer[0].SyncVersion);
            var query = ServiceProvider.GetRequiredService<IMessageRepository>();
            Assert.False(await query.CanReadAsync(first, 105));
            Assert.True(await query.CanReadAsync(second, 105));
        });
    }

    [Fact]
    public async Task Visibility_and_sparse_id_cursor_translate_to_bounded_SQL_Server_queries()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Generate SQL only; no connection to this database is opened.
            using var context = new ChatDbContext(new DbContextOptionsBuilder<ChatDbContext>()
                .UseSqlServer("Server=localhost;Database=ReviewSqlGeneration;Integrated Security=true;TrustServerCertificate=true").Options)
            { LazyServiceProvider = ServiceProvider.GetRequiredService<IAbpLazyServiceProvider>() };
            var query = context.Message.Where(new OwnerMessageSpecification(1, Guid.NewGuid()).ToExpression());
            var history = query.Where(x => x.Id < 104).OrderByDescending(x => x.Id).Select(x => x.Id).Take(21).ToQueryString();
            var latest = query.Where(x => x.Id > 100).OrderBy(x => x.Id).Select(x => x.Id).Take(21).ToQueryString();
            Assert.Contains("TOP", history);
            Assert.Contains("EXISTS", history);
            Assert.Contains("IsPrivate", history);
            Assert.Contains("ClearTime", history);
            Assert.Contains("HistoryFristTime", history);
            Assert.Contains("Deleted", history);
            Assert.Contains("< CAST(104 AS bigint)", history);
            Assert.Contains("> CAST(100 AS bigint)", latest);
            await Task.CompletedTask;
        });
    }
}
