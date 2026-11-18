using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.Json;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Ai;

public class AiJob(
    IAiResolver aiResolver,
    IMessageSender messageSender,
    ISessionUnitManager sessionUnitManager,
    IFollowManager followManager,
    IMessageRepository messageRepository,
    IJsonSerializer jsonSerializer,
    IBackgroundJobManager backgroundJobManager,
    IChatObjectManager chatObjectManager,
    IConfiguration configuration) : DomainService, IAsyncBackgroundJob<AiJobArg>, ITransientDependency
{
    protected IAiResolver AiResolver { get; } = aiResolver;
    protected IMessageSender MessageSender { get; } = messageSender;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IFollowManager FollowManager { get; } = followManager;
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    protected IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;

    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected IMessageRepository MessageRepository { get; } = messageRepository;
    protected IConfiguration Configuration { get; } = configuration;

    protected virtual IAiProvider GetProvider(string providerName)
    {
        var provider = AiResolver.GetProviderOrDefault(providerName);
        return LazyServiceProvider.LazyGetService(provider) as IAiProvider;
    }

    [UnitOfWork]
    public async Task ExecuteAsync(AiJobArg args)
    {
        Logger.LogInformation($"{nameof(AiJob)} is executed:{args}");

        //var message = await MessageReadOnlyRepository.GetAsync(args.MessageId);

        //if (message.IsRollbackMessage())
        //{
        //    Logger.LogInformation($"Message is rollback:{message}");
        //    return;
        //}

        var aiProvider = GetProvider(args.Provider);

        Logger.LogInformation($"AiProvider={aiProvider.GetProviderName()},Model={aiProvider.GetModel()}");

        // This is the final queue-protection boundary. Provider implementations
        // should honour their own cancellation tokens, but a missed token in a
        // network, event-bus, or database call must never keep ABP's serial
        // worker and its distributed lock forever.
        var timeoutSeconds = Math.Max(10, Configuration.GetValue("Ai:JobExecutionTimeoutSeconds", 330));
        var providerTask = aiProvider.HandleAsync(args.MessageId);
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));

        if (await Task.WhenAny(providerTask, timeoutTask) != providerTask)
        {
            ObserveLateProviderFailure(providerTask, args);
            throw new TimeoutException($"AI background job exceeded its {timeoutSeconds}s execution budget. Provider={args.Provider}; MessageId={args.MessageId}.");
        }

        await providerTask;

    }

    private void ObserveLateProviderFailure(Task providerTask, AiJobArg args)
    {
        _ = providerTask.ContinueWith(
            task => Logger.LogError(task.Exception, "AI provider completed with an error after its job execution budget expired. Provider={Provider}; MessageId={MessageId}", args.Provider, args.MessageId),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }
}
