using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategories.Dtos;
using IczpNet.Chat.Permissions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ChatObjectCategories;

/// <summary>
/// 聊天对象目录（分组）
/// </summary>
public class ChatObjectCategoryAppService(
    IRepository<ChatObjectCategory, Guid> repository,
    IChatObjectCategoryManager treeManager)
        : CrudTreeChatAppService<
        ChatObjectCategory,
        Guid,
        ChatObjectCategoryDetailDto,
        ChatObjectCategoryDto,
        ChatObjectCategoryGetListInput,
        ChatObjectCategoryCreateInput,
        ChatObjectCategoryUpdateInput,
        ChatObjectCategoryInfo>(repository, treeManager),
    IChatObjectCategoryAppService
{

    protected override string GetPolicyName { get; set; } = ChatPermissions.ChatObjectCategoryPermission.Default;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.ChatObjectCategoryPermission.Default;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.ChatObjectCategoryPermission.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.ChatObjectCategoryPermission.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.ChatObjectCategoryPermission.Delete;

    protected override async Task<IQueryable<ChatObjectCategory>> CreateFilteredQueryAsync(ChatObjectCategoryGetListInput input)
    {

        Assert.If(!input.IsEnabledParentId && input.ParentId.HasValue, "When [IsEnabledParentId]=false,then [ParentId] != null");

        return (await Repository.GetQueryableAsync())
            //.WhereIf(input.Depth.HasValue, (x) => x.Depth == input.Depth)
            .WhereIf(!input.ChatObjectTypeId.IsNullOrWhiteSpace(), (x) => x.ChatObjectTypeId == input.ChatObjectTypeId)
            .WhereIf(input.IsEnabledParentId, (x) => x.ParentId == input.ParentId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword));


    }
}
