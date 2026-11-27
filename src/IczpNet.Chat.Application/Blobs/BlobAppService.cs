using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Blobs.Dtos;
using IczpNet.Chat.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

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

    private async Task<bool> IsProtectedChatBlobAsync(Blob entity)
    {
        if (entity.IsStatic ||
            string.Equals(entity.Container, "chat-files", StringComparison.OrdinalIgnoreCase) ||
            entity.UploaderChatObjectId.HasValue ||
            entity.UploadSessionUnitId.HasValue ||
            entity.UploadSessionId.HasValue ||
            entity.StorageObjectId.HasValue ||
            entity.Status == BlobStatus.Bound ||
            entity.RefCount > 0)
        {
            return true;
        }

        var refRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<BlobMessageRef, Guid>>();
        return await refRepo.AnyAsync(r => r.BlobId == entity.Id);
    }

    protected override async Task CheckCreateAsync(BlobCreateInput input)
    {
        if (input != null &&
            input.IsPublic &&
            string.Equals(input.Container, "chat-files", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied)
                .WithData("Reason", "Chat attachment container cannot be created as public via generic Blob CRUD.");
        }

        await base.CheckCreateAsync(input);
    }

    protected override async Task CheckUpdateAsync(Guid id, Blob entity, BlobUpdateInput input)
    {
        var isProtected = await IsProtectedChatBlobAsync(entity);
        if (isProtected && input != null && (input.IsPublic || (entity.IsStatic && !input.IsStatic)))
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied)
                .WithData("Reason", "Chat or static attachment visibility/flags cannot be modified via generic Blob CRUD.");
        }

        await base.CheckUpdateAsync(id, entity, input);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var entity = await Repository.FindAsync(id);
        if (entity != null && await IsProtectedChatBlobAsync(entity))
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied)
                .WithData("Reason", "Chat or static attachment cannot be deleted directly via generic Blob CRUD.");
        }

        await base.DeleteAsync(id);
    }

    public override async Task DeleteManyAsync(List<Guid> idList)
    {
        await CheckDeletePolicyAsync();
        if (idList != null && idList.Count > 0)
        {
            var distinctIds = idList.Distinct().ToList();
            var entities = (await Repository.GetQueryableAsync())
                .Where(x => distinctIds.Contains(x.Id))
                .ToList();
            foreach (var entity in entities)
            {
                if (await IsProtectedChatBlobAsync(entity))
                {
                    throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied)
                        .WithData("Reason", "Chat or static attachment cannot be batch-deleted directly via generic Blob CRUD.");
                }
            }
        }

        await base.DeleteManyAsync(idList);
    }
}
