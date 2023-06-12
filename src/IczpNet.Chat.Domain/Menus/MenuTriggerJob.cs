using Castle.DynamicProxy;
using IczpNet.AbpCommons;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Menus
{
    public class MenuTriggerJob : AsyncBackgroundJob<MenuTriggerArgs>, ITransientDependency
    {
        protected IMenuManager MenuManager { get; }
        protected IUnitOfWorkManager UnitOfWorkManager { get; }
        protected IRepository<Menu, Guid> Repository { get; }

        public MenuTriggerJob(
            IMenuManager menuManager,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<Menu, Guid> repository)
        {
            MenuManager = menuManager;
            UnitOfWorkManager = unitOfWorkManager;
            Repository = repository;
        }

        //[UnitOfWork(true, IsolationLevel.ReadUncommitted)]
        [UnitOfWork]
        public override async Task ExecuteAsync(MenuTriggerArgs args)
        {
            using var uow = UnitOfWorkManager.Begin();

            var menu = await Repository.GetAsync(args.MenuId);

            if (!await MenuManager.IsCheckMenuAsync(menu))
            {
                Logger.LogWarning($@"MenuTriggerJob is not checked:MenuId={args.MenuId},IsEnabled={menu.Owner?.IsEnabled},IsDeveloper={menu.Owner?.IsDeveloper}");
                Logger.LogWarning($@"Developer.IsEnabled={menu.Owner?.Developer?.IsEnabled},Developer.PostUrl={menu.Owner?.Developer?.PostUrl},");
                return;
            }

            var typeName = ProxyUtil.GetUnproxiedType(this).FullName;

            var req = await MenuManager.SendToRemoteHostAsync(menu, name: typeName);

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
