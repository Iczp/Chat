using IczpNet.AbpTrees;
using IczpNet.AbpTrees.Statics;
using System.Threading.Tasks;
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

    public override Task DeleteAsync(long id)
    {
        return base.DeleteAsync(id);
    }
}

