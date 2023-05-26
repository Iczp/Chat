using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class SessionUnitIncrementJob : AsyncBackgroundJob<SessionUnitIncrementArgs>, ITransientDependency
    {
        protected IUnitOfWorkManager UnitOfWorkManager { get; }
        protected ISessionUnitManager SessionUnitManager { get; }

        public SessionUnitIncrementJob(
            IUnitOfWorkManager unitOfWorkManager,
            ISessionUnitManager sessionUnitManager)
        {
            UnitOfWorkManager = unitOfWorkManager;
            SessionUnitManager = sessionUnitManager;
        }

        public override async Task ExecuteAsync(SessionUnitIncrementArgs args)
        {
            using var uow = UnitOfWorkManager.Begin();

            var totalCount = await SessionUnitManager.IncremenetAsync(args);

            Logger.LogInformation($"SessionUnitIncrementJob Completed totalCount:{totalCount}.");
        }
    }
}
