using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class UpdateStatsForSessionUnitJob : AsyncBackgroundJob<UpdateStatsForSessionUnitArgs>, ITransientDependency
    {
        protected IMessageRepository MessageRepository { get; }
        protected ISessionUnitManager SessionUnitManager { get; }
        protected IUnitOfWorkManager UnitOfWorkManager { get; }

        public UpdateStatsForSessionUnitJob(
            IMessageRepository mssageRepository,
            ISessionUnitManager sessionUnitManager,
            IUnitOfWorkManager unitOfWorkManager)
        {
            MessageRepository = mssageRepository;
            SessionUnitManager = sessionUnitManager;
            UnitOfWorkManager = unitOfWorkManager;
        }

        public override async Task ExecuteAsync(UpdateStatsForSessionUnitArgs args)
        {
            using var uow = UnitOfWorkManager.Begin();

            var senderSessionUnit = await SessionUnitManager.GetAsync(args.SenderSessionUnitId);

            var message = await MessageRepository.GetAsync(args.MessageId);

            var result = await SessionUnitManager.BatchUpdateAsync(senderSessionUnit, message);

            Logger.LogInformation($"UpdateStatsForSessionUnitJob Completed {result}.");
        }
    }
}
