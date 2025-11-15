using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.AppVersions;

/// <inheritdoc />
public class AppVersionAppService(IAppVersionRepository repository) : CrudChatAppService<AppVersion, AppVersionDetailDto, AppVersionDto, Guid, AppVersionGetListInput, AppVersionCreateInput, AppVersionUpdateInput>(repository), IAppVersionAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.AppVersionPermissions.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.AppVersionPermissions.GetList;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.AppVersionPermissions.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.AppVersionPermissions.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.AppVersionPermissions.Delete;
    protected virtual string SetIsEnabledPolicyName { get; set; } = ChatPermissions.AppVersionPermissions.SetIsEnabled;

    /// <inheritdoc />
    protected override async Task<IQueryable<AppVersion>> CreateFilteredQueryAsync(AppVersionGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            //.WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword))
            ;
    }
}
