using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Counters;

public class ReadedCounterJob(IMessageRepository messageRepository, IUnitOfWorkManager unitOfWorkManager) : AsyncBackgroundJob<ReadedCounterArgs>, ITransientDependency
{
    protected IMessageRepository MessageRepository { get; } = messageRepository;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    [UnitOfWork]
    public override async Task ExecuteAsync(ReadedCounterArgs args)
    {
        using var uow = UnitOfWorkManager.Begin();

        var totalCount = await MessageRepository.IncrementReadedCountAsync(args.MessageIdList);

        Logger.LogInformation($"ReadedCounterJob Completed totalCount:{totalCount}.");
    }
}
