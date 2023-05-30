using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Menus
{
    public class MenuManager : TreeManager<Menu, Guid, MenuInfo>, IMenuManager
    {

        public MenuManager(IRepository<Menu, Guid> repository) : base(repository) { }

        protected override async Task CheckExistsByCreateAsync(Menu inputEntity)
        {
            Assert.If(await Repository.AnyAsync(x => x.Name == inputEntity.Name && x.OwnerId == inputEntity.OwnerId), $"Already exists Name:{inputEntity.Name}");
        }

        protected override async Task CheckExistsByUpdateAsync(Menu inputEntity)
        {
            Assert.If(await Repository.AnyAsync((x) => x.Name == inputEntity.Name && x.OwnerId == inputEntity.OwnerId && !x.Id.Equals(inputEntity.Id)), $" Name[{inputEntity.Name}] already such");
        }
    }
}
