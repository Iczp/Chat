using IczpNet.BizCrypts;
using IczpNet.Chat.Developers;
using IczpNet.Chat.HttpRequests;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
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

        [UnitOfWork]
        public override async Task ExecuteAsync(MenuTriggerArgs args)
        {
            //using var uow = UnitOfWorkManager.Begin();

            var req = await MenuManager.SendToRemoteHostAsync(args.MenuId, name: nameof(MenuTriggerJob));

            Logger.LogInformation($"MenuTriggerJob ReqId={req.Id},[GET,{req.StatusCode}],url={req.Url}");
        }
    }
}
