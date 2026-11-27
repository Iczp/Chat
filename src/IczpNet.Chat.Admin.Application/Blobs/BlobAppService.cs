using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Blobs.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.Blobs;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.Blobs;

/// <summary>
/// 附件与对象存储管理服务
/// </summary>
[Authorize(ChatAdminPermissions.Blobs.Default)]
[ExposeServices(typeof(BlobAppService), typeof(IBlobAdminAppService))]
public class BlobAppService(IBlobManager blobManager)
    : ApplicationService, IBlobAdminAppService
{
    protected IBlobManager BlobManager { get; } = blobManager;

    [Authorize(ChatAdminPermissions.Blobs.GetList)]
    public virtual async Task<PagedResultDto<BlobAdminDto>> GetListAsync(
        BlobAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new BlobAdminGetListInput();

        var result = await BlobManager.GetAdminListAsync(
            input.Keyword,
            input.Container,
            input.IsPublic,
            input.IsStatic,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<BlobAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.Blobs.GetDetail)]
    public virtual async Task<BlobAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await BlobManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.Blobs.GetList)]
    public virtual async Task<List<BlobAdminDto>> GetManyAsync(
        List<Guid> idList,
        CancellationToken cancellationToken = default)
    {
        if (idList == null || idList.Count == 0)
        {
            return [];
        }

        var results = new List<BlobAdminDto>();
        foreach (var id in idList.Distinct().Take(100))
        {
            var entity = await BlobManager.FindAsync(id);
            if (entity != null)
            {
                results.Add(MapToDto(entity));
            }
        }
        return results;
    }

    [Authorize(ChatAdminPermissions.Blobs.Update)]
    public virtual async Task<BlobAdminDto> UpdateAsync(
        Guid id,
        BlobAdminUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await BlobManager.UpdateAdminAsync(
            id,
            input.FileName,
            input.IsPublic,
            input.IsStatic,
            cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.Blobs.Delete)]
    public virtual Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return BlobManager.DeleteAdminAsync(id, cancellationToken);
    }

    private static BlobAdminDto MapToDto(Blob entity)
    {
        return new BlobAdminDto
        {
            Id = entity.Id,
            Container = entity.Container,
            Name = entity.Name,
            FileName = entity.FileName,
            FileSize = entity.FileSize,
            MimeType = entity.MimeType,
            Suffix = entity.Suffix,
            IsPublic = entity.IsPublic,
            IsStatic = entity.IsStatic,
            UploaderChatObjectId = entity.UploaderChatObjectId,
            RefCount = entity.RefCount,
            StorageObjectId = entity.StorageObjectId,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }
}
