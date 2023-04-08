using IczpNet.AbpTrees.Statics;
using IczpNet.Chat.ChatObjects;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.OfficialSections.Officials
{
    public class OfficialManager : ChatObjectManager, IOfficialManager
    {
        public OfficialManager(IChatObjectRepository repository) : base(repository)
        {
        }


        protected override async Task CheckExistsByCreateAsync(ChatObject inputEntity)
        {
            Assert.If(await Repository.AnyAsync(x => x.ObjectType == inputEntity.ObjectType && x.Name == inputEntity.Name), $"Already exists name:{inputEntity.Name},ObjectType:{inputEntity.ObjectType}");
        }

        protected override async Task CheckExistsByUpdateAsync(ChatObject inputEntity)
        {
            Assert.If(await Repository.AnyAsync((x) => x.ObjectType == inputEntity.ObjectType && x.Name == inputEntity.Name && !x.Id.Equals(inputEntity.Id)), $" Name[{inputEntity.Name}] already such,,ObjectType:{inputEntity.ObjectType}");
        }
    }
}
