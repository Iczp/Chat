using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.EntryValues.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.AbpCommons;
using IczpNet.Chat.Permissions;

namespace IczpNet.Chat.EntryValues;

/// <summary>
/// 条目值
/// </summary>
public class EntryValueAppService
    : CrudChatAppService<
        EntryValue,
        EntryValueDetailDto,
        EntryValueDto,
        Guid,
        EntryValueGetListInput,
        EntryValueCreateInput,
        EntryValueUpdateInput>,
    IEntryValueAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.EntryValuePermission.Default;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.EntryValuePermission.Default;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.EntryValuePermission.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.EntryValuePermission.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.EntryValuePermission.Delete;

    public EntryValueAppService(IRepository<EntryValue, Guid> repository) : base(repository)
    {
    }

    protected override async Task<IQueryable<EntryValue>> CreateFilteredQueryAsync(EntryValueGetListInput input)
    {
        return (await ReadOnlyRepository.GetQueryableAsync())
            .WhereIf(input.EntryNameId.HasValue, x => x.EntryNameId == input.EntryNameId)
            .WhereIf(input.IsOption.HasValue, x => x.IsOption == input.IsOption)
            .WhereIf(input.IsPublic.HasValue, x => x.IsPublic == input.IsPublic)
            .WhereIf(input.IsStatic.HasValue, x => x.IsStatic == input.IsStatic)
            .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Value.Contains(input.Keyword));
    }


    protected override async Task CheckCreateAsync(EntryValueCreateInput input)
    {
        Assert.If(await Repository.AnyAsync(x => x.Value.Equals(input.Value)), $"Already exists [{input.Value}] ");
        await base.CheckCreateAsync(input);
    }

    protected override async Task CheckUpdateAsync(Guid id, EntryValue entity, EntryValueUpdateInput input)
    {
        await base.CheckUpdateAsync(id, entity, input);
    }
}
