using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionBoxes;

/// <inheritdoc />
public class BoxAppService(IBoxRepository repository) : CrudChatAppService<Box, BoxDetailDto, BoxDto, Guid, BoxGetListInput, BoxCreateInput, BoxUpdateInput>(repository), IBoxAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.BoxPermissions.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.BoxPermissions.GetList;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.BoxPermissions.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.BoxPermissions.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.BoxPermissions.Delete;
    protected virtual string SetIsEnabledPolicyName { get; set; } = ChatPermissions.BoxPermissions.SetIsEnabled;

    /// <inheritdoc />
    protected override async Task<IQueryable<Box>> CreateFilteredQueryAsync(BoxGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword))
            ;
    }
}
