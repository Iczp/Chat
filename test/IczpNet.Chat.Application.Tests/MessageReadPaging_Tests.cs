using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageServices;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
using Xunit;

namespace IczpNet.Chat;

public class MessageReadPaging_Tests
{
    private sealed class Reader
    {
        public MessageAppService Service { get; }
        public IMessageManager Manager { get; }
        public IMessageRepository Repository { get; }
        public SessionUnitCacheItem Unit { get; }

        public Reader(long ownerId)
        {
            var constructor = typeof(MessageAppService).GetConstructors().Single();
            var arguments = constructor.GetParameters().Select(x => Substitute.For([x.ParameterType], [])).ToArray();
            Service = (MessageAppService)constructor.Invoke(arguments);
            Manager = arguments.OfType<IMessageManager>().Single();
            Repository = arguments.OfType<IMessageRepository>().Single();
            Unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), OwnerId = ownerId };

            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            var currentUser = Substitute.For<ICurrentUser>();
            var userId = Guid.NewGuid();
            currentUser.Id.Returns(userId);
            lazy.LazyGetRequiredService<ICurrentUser>().Returns(currentUser);
            var objects = Substitute.For<IChatObjectManager>();
            objects.GetIdListByUserIdAsync(userId).Returns(new List<long> { 1 });
            lazy.LazyGetRequiredService<IChatObjectManager>().Returns(objects);
            var units = Substitute.For<ISessionUnitManager>();
            units.GetCacheAsync(Unit.Id).Returns(Unit);
            lazy.LazyGetRequiredService<ISessionUnitManager>().Returns(units);
            var mapper = Substitute.For<IObjectMapper>();
            mapper.Map<MessageCacheItem, MessageFastDto>(Arg.Any<MessageCacheItem>())
                .Returns(call => new MessageFastDto { Id = call.Arg<MessageCacheItem>().Id });
            lazy.LazyGetService<IObjectMapper>(Arg.Any<Func<IServiceProvider, object>>()).Returns(mapper);
            Service.LazyServiceProvider = lazy;
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Foreign_unit_owner_is_rejected_before_message_manager_or_repository_calls(bool descending)
    {
        var reader = new Reader(ownerId: 2);
        await Assert.ThrowsAsync<AbpAuthorizationException>(async () =>
        {
            if (descending)
                await reader.Service.GetHistoryAsync(new MessageGetHistoryInput { SessionUnitId = reader.Unit.Id, MaxMessageId = 2000 });
            else
                await reader.Service.GetLatestAsync(new MessageGetLatestInput { SessionUnitId = reader.Unit.Id, MinMessageId = 100 });
        });
        Assert.Empty(reader.Manager.ReceivedCalls());
        Assert.Empty(reader.Repository.ReceivedCalls());
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 0)]
    [InlineData(true, 7)]
    [InlineData(false, 7)]
    [InlineData(true, 20)]
    [InlineData(false, 20)]
    [InlineData(true, 21)]
    [InlineData(false, 21)]
    public async Task Own_unit_pages_fetch_one_extra_row_use_last_returned_cursor_and_avoid_exact_count(bool descending, int fetchedCount)
    {
        var reader = new Reader(ownerId: 1);
        const int pageSize = 20;
        // Sparse message Ids expose off-by-one and discarded-extra-row cursor regressions.
        var ids = Enumerable.Range(0, fetchedCount).Select(i => 103L + i * 7).ToList();
        if (descending) ids.Reverse();
        reader.Manager.GetLatestAsync(reader.Unit, 100, pageSize + 1).Returns(_ => ids.ToList());
        reader.Manager.GetHistoryAsync(reader.Unit, 2000, pageSize + 1).Returns(_ => ids.ToList());
        var expected = ids.Take(pageSize).ToArray();
        reader.Manager.GetOrAddManyCacheAsync(Arg.Any<IEnumerable<long>>(),
                Arg.Any<Func<DistributedCacheEntryOptions>>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var requested = call.Arg<IEnumerable<long>>().ToArray();
                Assert.Equal(expected, requested);
                return requested.Select(id => new KeyValuePair<MessageCacheKey, MessageCacheItem>(
                    new MessageCacheKey(id), new MessageCacheItem { Id = id })).ToArray();
            });

        var result = descending
            ? await reader.Service.GetHistoryAsync(new MessageGetHistoryInput
            {
                SessionUnitId = reader.Unit.Id, MaxMessageId = 2000, MaxResultCount = pageSize
            })
            : await reader.Service.GetLatestAsync(new MessageGetLatestInput
            {
                SessionUnitId = reader.Unit.Id, MinMessageId = 100, MaxResultCount = pageSize
            });

        Assert.Equal(expected, result.Items.Select(x => x.Id));
        Assert.Equal(fetchedCount, result.TotalCount);
        var hasMore = fetchedCount > pageSize;
        Assert.Equal(hasMore, Assert.IsType<bool>(result.Extra.GetType().GetProperty("HasMore").GetValue(result.Extra)));
        var nextCursor = (long?)result.Extra.GetType().GetProperty("NextCursorId").GetValue(result.Extra);
        Assert.Equal(hasMore ? expected[^1] : (long?)null, nextCursor);
        if (hasMore)
        {
            Assert.Equal(result.Items.Count + 1, result.TotalCount);
            Assert.DoesNotContain(ids[^1], result.Items.Select(x => x.Id));
        }
        if (descending)
        {
            await reader.Manager.Received(1).GetHistoryAsync(reader.Unit, 2000, pageSize + 1);
            await reader.Manager.DidNotReceiveWithAnyArgs().GetLatestAsync(default, default, default);
        }
        else
        {
            await reader.Manager.Received(1).GetLatestAsync(reader.Unit, 100, pageSize + 1);
            await reader.Manager.DidNotReceiveWithAnyArgs().GetHistoryAsync(default, default, default);
        }
        await reader.Manager.DidNotReceiveWithAnyArgs().GetSessionMessageTotalCountAsync(default, default, default);
        Assert.Empty(reader.Repository.ReceivedCalls());
    }
}
