using Castle.DynamicProxy;
using IczpNet.AbpCommons;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Menus
{
    public class MenuTriggerJob : AsyncBackgroundJob<MenuTriggerArgs>, ITransientDependency
    {
        protected IMenuManager MenuManager { get; }
        protected IUnitOfWorkManager UnitOfWorkManager { get; }

        public MenuTriggerJob(
            IMenuManager menuManager, IUnitOfWorkManager unitOfWorkManager)
        {
            MenuManager = menuManager;
            UnitOfWorkManager = unitOfWorkManager;
        }

        [UnitOfWork]
        public override async Task ExecuteAsync(MenuTriggerArgs args)
        {
            using var uow = UnitOfWorkManager.Begin();

            var typeName = ProxyUtil.GetUnproxiedType(this).FullName;

            var req = await MenuManager.SendToRemoteHostAsync(args.MenuId, name: typeName);

            Logger.LogInformation($"MenuTriggerJob HttpReqquestId={req.Id},[GET,{req.StatusCode}],url={req.Url}");

            uow.OnCompleted(async () =>
            {
                //throw: BackgroundJob retry
                Assert.If(!req.IsSuccess, $"Error:{req.Message},HttpReqquestId={req.Id},url={req.Url}");
                await Task.CompletedTask;
            });
        }
    }
}
