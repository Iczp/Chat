using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Volo.Abp.Caching;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

public class MessageSessionSeqCache_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
    [Fact]
    public async Task Old_cached_message_and_quote_are_filled_from_database()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
            var sessionId = Guid.NewGuid();
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new Dictionary<string, object> { ["Id"] = 151L, ["SessionId"] = sessionId, ["SessionSeq"] = 8L });
            await MessageVisibilityQuery_Tests.InsertAsync<Message>(db, new Dictionary<string, object> { ["Id"] = 150L, ["SessionId"] = sessionId, ["SessionSeq"] = 7L });
            var cached = new MessageCacheItem { Id = 151, QuoteMessage = new MessageQuoteCacheItem { Id = 150 } };
            var constructor = typeof(MessageManager).GetConstructors().Single();
            var args = constructor.GetParameters().Select(x => Substitute.For([x.ParameterType], [])).ToArray();
            var repository = (IMessageRepository)args.Single(x => x is IMessageRepository);
            repository.GetQueryableAsync().Returns(db.Set<Message>().AsQueryable());
            var cache = (IDistributedCache<MessageCacheItem, MessageCacheKey>)args.Single(x => x is IDistributedCache<MessageCacheItem, MessageCacheKey>);
            cache.GetAsync(Arg.Any<MessageCacheKey>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(cached);
            var result = await ((MessageManager)constructor.Invoke(args)).GetCacheAsync(151);
            Assert.Equal(8, result.SessionSeq);
            Assert.Equal(7, result.QuoteMessage.SessionSeq);
        });
    }
}
