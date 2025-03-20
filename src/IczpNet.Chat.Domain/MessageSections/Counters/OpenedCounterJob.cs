using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Counters;

public class OpenedCounterJob(IMessageRepository messageRepository, IUnitOfWorkManager unitOfWorkManager) : AsyncBackgroundJob<OpenedCounterArgs>, ITransientDependency
{
    protected IMessageRepository MessageRepository { get; } = messageRepository;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    [UnitOfWork]
    public override async Task ExecuteAsync(OpenedCounterArgs args)
    {
        using var uow = UnitOfWorkManager.Begin();

        var totalCount = await MessageRepository.IncrementOpenedCountAsync(args.MessageIdList);

        Logger.LogInformation($"OpenedCounterJob Completed totalCount:{totalCount}.");
    }
}
