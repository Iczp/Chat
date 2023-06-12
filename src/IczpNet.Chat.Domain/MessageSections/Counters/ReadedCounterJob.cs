using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Counters
{
    public class ReadedCounterJob : AsyncBackgroundJob<ReadedCounterArgs>, ITransientDependency
    {
        protected IMessageRepository MessageRepository { get; }
        protected IUnitOfWorkManager UnitOfWorkManager { get; }

        public ReadedCounterJob(IMessageRepository messageRepository, IUnitOfWorkManager unitOfWorkManager)
        {
            MessageRepository = messageRepository;
            UnitOfWorkManager = unitOfWorkManager;
        }

        [UnitOfWork]
        public override async Task ExecuteAsync(ReadedCounterArgs args)
        {
            using var uow = UnitOfWorkManager.Begin();

            var totalCount = await MessageRepository.IncrementReadedCountAsync(args.MessageIdList);

            Logger.LogInformation($"ReadedCounterJob Completed totalCount:{totalCount}.");
        }
    }
}
