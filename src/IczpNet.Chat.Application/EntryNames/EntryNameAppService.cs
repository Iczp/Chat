using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.EntryNames.Dtos;
using IczpNet.Chat.Permissions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.EntryNames;

/// <summary>
/// 条目（属性）
/// </summary>
public class EntryNameAppService
    : CrudTreeChatAppService<
        EntryName,
        Guid,
        EntryNameDetailDto,
        EntryNameDto,
        EntryNameGetListInput,
        EntryNameCreateInput,
        EntryNameUpdateInput>,
    IEntryNameAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.EntryNamePermission.Default;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.EntryNamePermission.Default;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.EntryNamePermission.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.EntryNamePermission.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.EntryNamePermission.Delete;

    public EntryNameAppService(
        IRepository<EntryName, Guid> repository,
        IEntryNameManager entryNameManager) : base(repository, entryNameManager)
    {
    }

    protected override async Task<IQueryable<EntryName>> CreateFilteredQueryAsync(EntryNameGetListInput input)
    {
        return (await ReadOnlyRepository.GetQueryableAsync())
            .WhereIf(!input.InputType.IsNullOrEmpty(), x => x.InputType.StartsWith(input.InputType))
            .WhereIf(input.IsChoice.HasValue, x => x.IsChoice == input.IsChoice)
            .WhereIf(input.IsPublic.HasValue, x => x.IsPublic == input.IsPublic)
            .WhereIf(input.IsStatic.HasValue, x => x.IsStatic == input.IsStatic)
            .WhereIf(input.IsUniqued.HasValue, x => x.IsUniqued == input.IsUniqued)

            .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Name.Contains(input.Keyword));
    }


    //protected override async Task CheckCreateAsync(EntryNameCreateInput input)
    //{
    //    Assert.If(await Repository.AnyAsync(x => x.Title.Equals(input.Title)), $"Already exists [{input.Title}] ");
    //    await base.CheckCreateAsync(input);
    //}

    //protected override async Task CheckUpdateAsync(Guid id, EntryName entity, EntryNameUpdateInput input)
    //{
    //    await base.CheckUpdateAsync(id, entity, input);
    //}


}
