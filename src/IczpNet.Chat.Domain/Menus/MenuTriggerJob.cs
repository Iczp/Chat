using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Menus
{
    public class MenuTriggerJob : AsyncBackgroundJob<MenuTriggerArgs>, ITransientDependency
    {
        protected IUnitOfWorkManager UnitOfWorkManager { get; }
        protected IMenuManager MenuManager { get; }

        public MenuTriggerJob(
            IUnitOfWorkManager unitOfWorkManager,
            IMenuManager menuManager)
        {
            UnitOfWorkManager = unitOfWorkManager;
            MenuManager = menuManager;
        }

        public override async Task ExecuteAsync(MenuTriggerArgs args)
        {
            using var uow = UnitOfWorkManager.Begin();

            var totalCount = await MenuManager.TriggerAsync(args);

            Logger.LogInformation($"MenuTriggerJob Completed totalCount:{totalCount}.");
        }
    }
}
