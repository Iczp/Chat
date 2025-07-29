using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Counters;

public class DeletedCounterJob(IMessageRepository messageRepository, IUnitOfWorkManager unitOfWorkManager) : AsyncBackgroundJob<DeletedCounterArgs>, ITransientDependency
{
    protected IMessageRepository MessageRepository { get; } = messageRepository;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    [UnitOfWork]
    public override async Task ExecuteAsync(DeletedCounterArgs args)
    {
        using var uow = UnitOfWorkManager.Begin();

        var totalCount = await MessageRepository.IncrementDeletedCountAsync(args.MessageIdList);

        Logger.LogInformation($"DeletedCounterJob Completed totalCount:{totalCount}.");
    }
}
