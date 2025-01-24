using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
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
    IChatObjectManager chatObjectManager) : DomainService, IAsyncBackgroundJob<AiJobArg>, ITransientDependency
{
    protected IAiResolver AiResolver { get; } = aiResolver;
    protected IMessageSender MessageSender { get; } = messageSender;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IFollowManager FollowManager { get; } = followManager;
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    protected IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;

    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected IMessageRepository MessageRepository { get; } = messageRepository;

    protected virtual IAiProvider GetProvider(string providerName)
    {
        var provider = AiResolver.GetProviderOrDefault(providerName);
        return LazyServiceProvider.LazyGetService(provider) as IAiProvider;
    }

    [UnitOfWork]
    public async Task ExecuteAsync(AiJobArg args)
    {
        Logger.LogInformation($"{nameof(AiJob)} is executed:{args}");

        //var message = await MessageRepository.GetAsync(args.MessageId);

        //if (message.IsRollbackMessage())
        //{
        //    Logger.LogInformation($"Message is rollback:{message}");
        //    return;
        //}

        var aiProvider = GetProvider(args.Provider);

        Logger.LogInformation($"AiProvider={aiProvider.GetProviderName()},Model={aiProvider.GetModel()}");

        await aiProvider.HandleAsync(args.MessageId);

    }
}
