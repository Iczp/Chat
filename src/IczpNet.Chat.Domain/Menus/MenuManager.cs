using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Menus
{
    public class MenuManager : TreeManager<Menu, Guid, MenuInfo>, IMenuManager
    {
        protected IBackgroundJobManager BackgroundJobManager { get; }

        public MenuManager(
            IRepository<Menu, Guid> repository,
            IBackgroundJobManager backgroundJobManager) : base(repository)
        {
            BackgroundJobManager = backgroundJobManager;
        }

        protected override async Task CheckExistsByCreateAsync(Menu inputEntity)
        {
            Assert.If(await Repository.AnyAsync(x => x.Name == inputEntity.Name && x.OwnerId == inputEntity.OwnerId), $"Already exists Name:{inputEntity.Name}");
        }

        protected override async Task CheckExistsByUpdateAsync(Menu inputEntity)
        {
            Assert.If(await Repository.AnyAsync((x) => x.Name == inputEntity.Name && x.OwnerId == inputEntity.OwnerId && !x.Id.Equals(inputEntity.Id)), $" Name[{inputEntity.Name}] already such");
        }


        public virtual Task<string> TriggerAsync(MenuTriggerArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
