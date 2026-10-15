using IczpNet.Chat.MessageSections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Xunit;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Timing;

namespace IczpNet.Chat.MessageSections.Tests;

public class ArticleScheduleExecution_Tests
{
    [Fact]
    public async Task Retry_keeps_the_message_idempotency_key_and_completed_trigger_is_a_noop()
    {
        var constructor = typeof(ArticleScheduleManager).GetConstructors().Single();
        var arguments = constructor.GetParameters().Select(parameter => Substitute.For([parameter.ParameterType], [])).ToArray();
        var repository = (IRepository<ScheduledArticleMessage, Guid>)arguments.Single(x => x is IRepository<ScheduledArticleMessage, Guid>);
        var sender = (IMessageSender)arguments.Single(x => x is IMessageSender);
        var now = DateTime.UtcNow;
        var input = new MessageInput<ArticleContentInput> { ClientMessageId = "untrusted-caller-key", Content = new ArticleContentInput { Items = [new ArticleItemInfo { Title = "Article", OriginalUrl = "https://example.com" }] } };
        var task = new ScheduledArticleMessage(Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(), JsonSerializer.Serialize(input), now.AddMinutes(-1));
        repository.FindAsync(task.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(task);
        repository.GetAsync(task.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(task);
        var distributedLock = (IAbpDistributedLock)arguments.Single(x => x is IAbpDistributedLock);
        distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Returns(Substitute.For<IAbpDistributedLockHandle>());
        ((IUnitOfWorkManager)arguments.Single(x => x is IUnitOfWorkManager)).Begin(Arg.Any<AbpUnitOfWorkOptions>(), true).Returns(Substitute.For<IUnitOfWork>());
        ((ISessionUnitManager)arguments.Single(x => x is ISessionUnitManager)).GetCacheAsync(task.SenderSessionUnitId)
            .Returns(new SessionUnitCacheItem { Id = task.SenderSessionUnitId, OwnerId = 1, OwnerObjectType = ChatObjectTypeEnums.Subscription, IsEnabled = true });
        ((IRepository<ChatObject, long>)arguments.Single(x => x is IRepository<ChatObject, long>)).GetAsync(1, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(new ChatObject("account", "account", new IczpNet.Chat.ChatObjectTypes.ChatObjectType("subscription"), null));
        ((IArticleScheduleAuthorization)arguments.Single(x => x is IArticleScheduleAuthorization)).AuthorizeAsync(task.RequestedBy, 1).Returns(new ClaimsPrincipal());
        for (var index = 0; index < arguments.Length; index++)
            if (constructor.GetParameters()[index].ParameterType == typeof(IConfiguration))
                arguments[index] = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { ["Chat:ArticleSchedules:Enabled"] = "true" }).Build();
        var manager = (ArticleScheduleManager)constructor.Invoke(arguments);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        var clock = Substitute.For<IClock>(); clock.Now.Returns(now);
        lazy.LazyGetRequiredService<IClock>().Returns(clock);
        lazy.LazyGetService<ILogger>(Arg.Any<Func<IServiceProvider, ILogger>>()).Returns(NullLogger.Instance);
        manager.LazyServiceProvider = lazy;
        var keys = new List<string>();
        sender.SendArticleContentMessageAsync(Arg.Any<SessionUnitCacheItem>(), Arg.Any<MessageInput<ArticleContentInput>>()).Returns(call =>
        {
            keys.Add(call.Arg<MessageInput<ArticleContentInput>>().ClientMessageId);
            if (keys.Count == 1) throw new UserFriendlyException("Temporary failure");
            return Task.FromResult(new MessageInfo<ArticleContentInfo> { Id = 42 });
        });
        var args = new ArticleScheduleJobArgs { Id = task.Id, Version = 1 };
        await Assert.ThrowsAsync<UserFriendlyException>(() => manager.ExecuteAsync(args));
        Assert.Equal(ArticleScheduleStatus.Failed, task.Status);
        await manager.ExecuteAsync(args);
        await manager.ExecuteAsync(args);
        Assert.Equal(new[] { new Ulid(task.Id).ToString(), new Ulid(task.Id).ToString() }, keys);
        Assert.All(keys, key => Assert.Equal(26, key.Length));
        Assert.Equal(task.Id, Ulid.Parse(keys[0]).ToGuid());
        Assert.Equal(42, task.MessageId);
        Assert.Equal(ArticleScheduleStatus.Sent, task.Status);
        Assert.Equal(2, task.Attempts);
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Cancelled_or_rescheduled_trigger_never_sends(bool cancelled)
    {
        var constructor = typeof(ArticleScheduleManager).GetConstructors().Single();
        var arguments = constructor.GetParameters().Select(_ => (object)null).ToArray();
        var repository = Substitute.For<IRepository<ScheduledArticleMessage, Guid>>();
        var distributedLock = Substitute.For<IAbpDistributedLock>();
        var handle = Substitute.For<IAbpDistributedLockHandle>();
        distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Returns(handle);
        var unitsOfWork = Substitute.For<IUnitOfWorkManager>();
        unitsOfWork.Begin(Arg.Any<AbpUnitOfWorkOptions>(), true).Returns(Substitute.For<IUnitOfWork>());
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { ["Chat:ArticleSchedules:Enabled"] = "true" }).Build();
        var sender = Substitute.For<IMessageSender>();
        for (var index = 0; index < arguments.Length; index++)
        {
            var type = constructor.GetParameters()[index].ParameterType;
            arguments[index] = type == typeof(IRepository<ScheduledArticleMessage, Guid>) ? repository :
                type == typeof(IAbpDistributedLock) ? distributedLock : type == typeof(IUnitOfWorkManager) ? unitsOfWork :
                type == typeof(IConfiguration) ? configuration : type == typeof(IMessageSender) ? sender :
                type == typeof(Volo.Abp.Threading.ICancellationTokenProvider) ? Substitute.For<Volo.Abp.Threading.ICancellationTokenProvider>() : null;
        }
        var manager = (ArticleScheduleManager)constructor.Invoke(arguments);
        var task = new ScheduledArticleMessage(Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(), "{}", DateTime.UtcNow.AddHours(1));
        if (cancelled) task.Cancel(); else task.Reschedule(DateTime.UtcNow.AddHours(2));
        repository.FindAsync(task.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(task);
        await manager.ExecuteAsync(new ArticleScheduleJobArgs { Id = task.Id, Version = 1 });
        await sender.DidNotReceiveWithAnyArgs().SendArticleContentMessageAsync(default, default);
        await handle.Received(1).DisposeAsync();
    }
}
