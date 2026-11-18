using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NSubstitute;
using Volo.Abp.Caching;
using Xunit;

namespace IczpNet.Chat;

public class MessageSenderHydration_Tests
{
    [Fact]
    public async Task Cached_messages_and_quotes_share_one_batch_sender_lookup()
    {
        var constructor = typeof(MessageManager).GetConstructors().Single();
        var args = constructor.GetParameters().Select(p => p.ParameterType == typeof(IOptions<MessageOptions>)
            ? (object)Microsoft.Extensions.Options.Options.Create(new MessageOptions()) : Substitute.For([p.ParameterType], [])).ToArray();
        var manager = (MessageManager)constructor.Invoke(args);
        var cache = args.OfType<IDistributedCache<MessageCacheItem, MessageCacheKey>>().Single();
        var members = args.OfType<ISessionUnitManager>().Single();
        var id = Guid.NewGuid();
        var cached = new MessageCacheItem { Id = 5, SenderSessionUnitId = id,
            QuoteMessage = new MessageQuoteCacheItem { Id = 1, SenderSessionUnitId = id } };
        cache.GetOrAddManyAsync(Arg.Any<IEnumerable<MessageCacheKey>>(),
            Arg.Any<Func<IEnumerable<MessageCacheKey>, Task<List<KeyValuePair<MessageCacheKey, MessageCacheItem>>>>>(),
            Arg.Any<Func<DistributedCacheEntryOptions>>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([new(new MessageCacheKey(5), cached)]);
        members.GetCacheManyAsync(Arg.Any<List<Guid>>()).Returns([new(id, new SessionUnitCacheItem { Id = id, OwnerId = 7 })]);
        members.MapToSenderAsync(Arg.Any<List<SessionUnitCacheItem>>())
            .Returns([new SessionUnitSenderInfo { Id = id, Owner = new ChatObjectInfo { Id = 7, Thumbnail = "/avatar.png" } }]);
        await manager.GetOrAddManyCacheAsync([5]);
        Assert.Equal("/avatar.png", cached.SenderSessionUnit.Owner.Thumbnail);
        Assert.Same(cached.SenderSessionUnit, cached.QuoteMessage.SenderSessionUnit);
        await members.Received(1).GetCacheManyAsync(Arg.Is<List<Guid>>(ids => ids.SequenceEqual(new[] { id })));
        await members.Received(1).MapToSenderAsync(Arg.Any<List<SessionUnitCacheItem>>());
        // A complete warm response needs no further sender lookups.
        await manager.GetOrAddManyCacheAsync([5]);
        await members.Received(1).GetCacheManyAsync(Arg.Any<List<Guid>>());
    }
}
