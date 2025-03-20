using IczpNet.AbpTrees;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionSections.SessionPermissionGroups;

public class SessionPermissionGroupManager : TreeManager<SessionPermissionGroup, long, SessionPermissionGroupInfo>, ISessionPermissionGroupManager
{
    public SessionPermissionGroupManager(IRepository<SessionPermissionGroup, long> repository) : base(repository)
    {
    }

    //public override async Task<IQueryable<SessionPermissionGroup>> QueryCurrentAndAllChildsAsync(IEnumerable<string> fullPaths)
    //{

    //    var entityPredicate = PredicateBuilder.New<SessionPermissionGroup>();

    //    foreach (var fullPath in fullPaths)
    //    {
    //        entityPredicate = entityPredicate.Or(x => (x.FullPath + "/").StartsWith(fullPath + "/"));
    //    }

    //    var entityIdQuery = (await Repository.GetQueryableAsync())
    //        .Where(entityPredicate)
    //    ;

    //    //Logger.LogDebug("entityIdQuery:\r\n" + entityIdQuery.ToQueryString());
    //    //Logger.LogDebug("entityIdQuery:\r\n" + string.Join(",", entityIdQuery.ToList()));

    //    return entityIdQuery;
    //}

}

