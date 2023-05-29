using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Counters
{
    public class OpenedCounterJob : AsyncBackgroundJob<OpenedCounterArgs>, ITransientDependency
    {
        protected IMessageRepository MessageRepository { get; }
        protected IUnitOfWorkManager UnitOfWorkManager { get; }

        public OpenedCounterJob(IMessageRepository messageRepository, IUnitOfWorkManager unitOfWorkManager)
        {
            MessageRepository = messageRepository;
            UnitOfWorkManager = unitOfWorkManager;
        }

        public override async Task ExecuteAsync(OpenedCounterArgs args)
        {
            using var uow = UnitOfWorkManager.Begin();

            var totalCount = await MessageRepository.IncrementOpenedCountAsync(args.MessageIdList);

            Logger.LogInformation($"OpenedCounterJob Completed totalCount:{totalCount}.");
        }
    }
}
