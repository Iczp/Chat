using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class UpdateStatsForSessionUnitJob : AsyncBackgroundJob<UpdateStatsForSessionUnitArgs>, ITransientDependency
    {
        protected IMessageRepository MessageRepository { get; }
        protected ISessionUnitManager SessionUnitManager { get; }

        public UpdateStatsForSessionUnitJob(
            IMessageRepository mssageRepository,
            ISessionUnitManager sessionUnitManager)
        {
            MessageRepository = mssageRepository;
            SessionUnitManager = sessionUnitManager;
        }

        public override async Task ExecuteAsync(UpdateStatsForSessionUnitArgs args)
        {
            var senderSessionUnit = await SessionUnitManager.GetAsync(args.SenderSessionUnitId);

            var message = await MessageRepository.GetAsync(args.MessageId);

            var result = await SessionUnitManager.BatchUpdateAsync(senderSessionUnit, message, args.ReceiverSessionUnitId);

            Logger.LogInformation($"Completed {result}.");
        }
    }
}
