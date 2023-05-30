using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ActionMenus
{
    public class ActionMenuManager : TreeManager<ActionMenu, long, ActionMenuInfo>, IActionMenuManager
    {

     
        public ActionMenuManager(IRepository<ActionMenu,long> repository) : base(repository)
        {

        }

        

        protected override async Task CheckExistsByCreateAsync(ActionMenu inputEntity)
        {
            Assert.If( await Repository.AnyAsync(x => x.Name == inputEntity.Name && x.OwnerId == inputEntity.OwnerId), $"Already exists Name:{inputEntity.Name}");
        }

        protected override async Task CheckExistsByUpdateAsync(ActionMenu inputEntity)
        {
            Assert.If( await Repository.AnyAsync((x) => x.Name == inputEntity.Name && x.OwnerId == inputEntity.OwnerId && !x.Id.Equals(inputEntity.Id)), $" Name[{inputEntity.Name}] already such");
        }

      
    }
}
