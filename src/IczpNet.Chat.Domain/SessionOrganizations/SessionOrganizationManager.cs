using IczpNet.AbpTrees;
using IczpNet.AbpTrees.Statics;
using IczpNet.Chat.SessionOrganizationRoles;
using IczpNet.Chat.SessionUnitOrganizations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionOrganizations;

public class SessionOrganizationManager : TreeManager<SessionOrganization, long, SessionOrganizationInfo>, ISessionOrganizationManager
{
    public SessionOrganizationManager(IRepository<SessionOrganization, long> repository) : base(repository)
    {
    }

    protected override async Task CheckExistsByCreateAsync(SessionOrganization inputEntity)
    {
        Assert.If(await Repository.AnyAsync(x => x.SessionId == inputEntity.SessionId && x.Name == inputEntity.Name), $"Already exists name:{inputEntity.Name},sessionId:{inputEntity.SessionId}"); 
    }

    protected override async Task CheckExistsByUpdateAsync(SessionOrganization inputEntity)
    {
        Assert.If(await Repository.AnyAsync((x) => x.SessionId == inputEntity.SessionId && x.Name == inputEntity.Name && !x.Id.Equals(inputEntity.Id)), $" Name[{inputEntity.Name}] already such,,sessionId:{inputEntity.SessionId}");
    }

    public override async Task DeleteAsync(long id)
    {
        if (await Repository.AnyAsync(x => x.ParentId == id))
        {
            throw new UserFriendlyException("请先删除或迁移子组织后再删除当前组织。");
        }

        var unitOrgRepo = LazyServiceProvider.LazyGetService<IRepository<SessionUnitOrganization>>();
        if (unitOrgRepo != null && await unitOrgRepo.AnyAsync(x => x.SessionOrganizationId == id))
        {
            throw new UserFriendlyException("当前组织下仍有成员，请先将成员移出组织或设为未分组后再删除。");
        }

        var orgRoleRepo = LazyServiceProvider.LazyGetService<IRepository<SessionOrganizationRole>>();
        if (orgRoleRepo != null && await orgRoleRepo.AnyAsync(x => x.SessionOrganizationId == id))
        {
            throw new UserFriendlyException("当前组织已绑定角色，请先解除角色绑定后再删除。");
        }

        await base.DeleteAsync(id);
    }
}

