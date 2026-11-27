using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.InvitationCodes.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.InvitationCodes;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.InvitationCodes;

/// <summary>
/// 邀请码管理服务
/// </summary>
[Authorize(ChatAdminPermissions.InvitationCodes.Default)]
[ExposeServices(typeof(InvitationCodeAppService), typeof(IInvitationCodeAdminAppService))]
public class InvitationCodeAppService(IInvitationCodeManager invitationCodeManager)
    : ApplicationService, IInvitationCodeAdminAppService
{
    protected IInvitationCodeManager InvitationCodeManager { get; } = invitationCodeManager;

    [Authorize(ChatAdminPermissions.InvitationCodes.GetList)]
    public virtual async Task<PagedResultDto<InvitationCodeAdminListItemDto>> GetListAsync(
        InvitationCodeAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new InvitationCodeAdminGetListInput();

        var result = await InvitationCodeManager.GetAdminListAsync(
            input.Keyword,
            input.OwnerId,
            input.IsEnabled,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<InvitationCodeAdminListItemDto>(
            result.TotalCount,
            result.Items.Select(MapToListItemDto).ToList());
    }

    [Authorize(ChatAdminPermissions.InvitationCodes.GetDetail)]
    public virtual async Task<InvitationCodeAdminDetailDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await InvitationCodeManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDetailDto(entity);
    }

    [Authorize(ChatAdminPermissions.InvitationCodes.Create)]
    public virtual async Task<InvitationCodeAdminDetailDto> CreateAsync(
        InvitationCodeAdminCreateInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await InvitationCodeManager.CreateAdminAsync(
            input.Title,
            input.OwnerId,
            input.ExpirationTime,
            cancellationToken);
        return MapToDetailDto(entity);
    }

    [Authorize(ChatAdminPermissions.InvitationCodes.Update)]
    public virtual async Task<InvitationCodeAdminDetailDto> UpdateAsync(
        Guid id,
        InvitationCodeAdminUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await InvitationCodeManager.UpdateAdminAsync(
            id,
            input.Title,
            input.ExpirationTime,
            input.IsEnabled,
            cancellationToken);
        return MapToDetailDto(entity);
    }

    [Authorize(ChatAdminPermissions.InvitationCodes.SetIsEnabled)]
    public virtual async Task<InvitationCodeAdminDetailDto> SetIsEnabledAsync(
        Guid id,
        InvitationCodeAdminSetIsEnabledInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await InvitationCodeManager.SetIsEnabledAdminAsync(id, input.IsEnabled, cancellationToken);
        return MapToDetailDto(entity);
    }

    [Authorize(ChatAdminPermissions.InvitationCodes.Delete)]
    public virtual Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return InvitationCodeManager.DeleteAdminAsync(id, cancellationToken);
    }

    private static string MaskCode(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return string.Empty;
        }
        if (code.Length <= 4)
        {
            return new string('*', code.Length);
        }
        return string.Concat(code.AsSpan(0, 2), new string('*', code.Length - 4), code.AsSpan(code.Length - 2));
    }

    private static InvitationCodeAdminListItemDto MapToListItemDto(InvitationCode entity)
    {
        return new InvitationCodeAdminListItemDto
        {
            Id = entity.Id,
            Title = entity.Title,
            MaskedCode = MaskCode(entity.Code),
            ExpirationTime = entity.ExpirationTime,
            OwnerId = entity.OwnerId,
            IsEnabled = entity.IsEnabled,
            CreationTime = entity.CreationTime
        };
    }

    private static InvitationCodeAdminDetailDto MapToDetailDto(InvitationCode entity)
    {
        return new InvitationCodeAdminDetailDto
        {
            Id = entity.Id,
            Title = entity.Title,
            MaskedCode = MaskCode(entity.Code),
            Code = entity.Code,
            ExpirationTime = entity.ExpirationTime,
            OwnerId = entity.OwnerId,
            IsEnabled = entity.IsEnabled,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }
}
