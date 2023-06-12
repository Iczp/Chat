using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Counters
{
    public class FavoritedCounterJob : AsyncBackgroundJob<FavoritedCounterArgs>, ITransientDependency
    {
        protected IMessageRepository MessageRepository { get; }
        protected IUnitOfWorkManager UnitOfWorkManager { get; }

        public FavoritedCounterJob(IMessageRepository messageRepository, IUnitOfWorkManager unitOfWorkManager)
        {
            MessageRepository = messageRepository;
            UnitOfWorkManager = unitOfWorkManager;
        }

        [UnitOfWork]
        public override async Task ExecuteAsync(FavoritedCounterArgs args)
        {
            using var uow = UnitOfWorkManager.Begin();

            var totalCount = await MessageRepository.IncrementFavoritedCountAsync(args.MessageIdList);

            Logger.LogInformation($"FavoritedCounterJob Completed totalCount:{totalCount}.");
        }
    }
}
