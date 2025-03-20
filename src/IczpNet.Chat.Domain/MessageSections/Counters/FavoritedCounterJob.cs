using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Counters;

public class FavoritedCounterJob(IMessageRepository messageRepository, IUnitOfWorkManager unitOfWorkManager) : AsyncBackgroundJob<FavoritedCounterArgs>, ITransientDependency
{
    protected IMessageRepository MessageRepository { get; } = messageRepository;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    [UnitOfWork]
    public override async Task ExecuteAsync(FavoritedCounterArgs args)
    {
        using var uow = UnitOfWorkManager.Begin();

        var totalCount = await MessageRepository.IncrementFavoritedCountAsync(args.MessageIdList);

        Logger.LogInformation($"FavoritedCounterJob Completed totalCount:{totalCount}.");
    }
}
