using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.Json;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Developers;

public class DeveloperJob(
    ISessionUnitManager sessionUnitManager,
    IFollowManager followManager,
    IMessageRepository messageRepository,
    IJsonSerializer jsonSerializer,
    IBackgroundJobManager backgroundJobManager,
    IChatObjectManager chatObjectManager,
    IDeveloperManager developerManager) : DomainService, IAsyncBackgroundJob<DeveloperJobArg>, ITransientDependency
{
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IFollowManager FollowManager { get; } = followManager;
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    protected IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;
    protected IDeveloperManager DeveloperManager { get; } = developerManager;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected IMessageRepository MessageRepository { get; } = messageRepository;

    [UnitOfWork]
    public async Task ExecuteAsync(DeveloperJobArg args)
    {
        Logger.LogInformation($"{nameof(DeveloperJob)} is executed:{args}");

        var message = await MessageRepository.GetAsync(args.MessageId);

        if (message.MessageType == MessageTypes.Text)
        {


        }

        var replySessionUnit = await SessionUnitManager.FindAsync(message.ReceiverId.Value, message.SenderId.Value);
    }
}
