using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Blobs.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.Permissions;

namespace IczpNet.Chat.Blobs;

public class BlobAppService
    : CrudChatAppService<
        Blob,
        BlobDetailDto,
        BlobDto,
        Guid,
        BlobGetListInput,
        BlobCreateInput,
        BlobUpdateInput>,
    IBlobAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.BlobPermission.Default;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.BlobPermission.Default;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.BlobPermission.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.BlobPermission.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.BlobPermission.Delete;

    public BlobAppService(IRepository<Blob, Guid> repository) : base(repository)
    {
    }

    protected override async Task<IQueryable<Blob>> CreateFilteredQueryAsync(BlobGetListInput input)
    {
        return (await ReadOnlyRepository.GetQueryableAsync())
            .WhereIf(input.IsPublic.HasValue, x => x.IsPublic == input.IsPublic)
            .WhereIf(input.IsStatic.HasValue, x => x.IsStatic == input.IsStatic)
            .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.FileName.Contains(input.Keyword));
    }


    protected override async Task CheckCreateAsync(BlobCreateInput input)
    {
        //Assert.If(await Repository.AnyAsync(x => x.Value.Equals(input.Value)), $"Already exists [{input.Value}] ");
        await base.CheckCreateAsync(input);
    }

    protected override async Task CheckUpdateAsync(Guid id, Blob entity, BlobUpdateInput input)
    {
        await base.CheckUpdateAsync(id, entity, input);
    }
}
