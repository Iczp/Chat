using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.Admin.ScanCodes.Dtos;
using IczpNet.Chat.ScanCodes;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.ScanCodes;

/// <summary>
/// 扫码服务流水管理服务
/// </summary>
[Authorize(ChatAdminPermissions.ScanCodes.Default)]
[ExposeServices(typeof(ScanCodeAppService), typeof(IScanCodeAdminAppService))]
public class ScanCodeAppService(IScanCodeManager scanCodeManager)
    : ApplicationService, IScanCodeAdminAppService
{
    protected IScanCodeManager ScanCodeManager { get; } = scanCodeManager;

    [Authorize(ChatAdminPermissions.ScanCodes.GetList)]
    public virtual async Task<PagedResultDto<ScanCodeAdminDto>> GetListAsync(
        ScanCodeAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new ScanCodeAdminGetListInput();

        var result = await ScanCodeManager.GetAdminListAsync(
            input.Keyword,
            input.HandlerFullName,
            input.Success,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<ScanCodeAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.ScanCodes.GetDetail)]
    public virtual async Task<ScanCodeAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await ScanCodeManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.ScanCodes.Simulate)]
    public virtual async Task<ScanCodeAdminDto> ScanAsync(ScanCodeAdminSimulateInput input)
    {
        var entity = await ScanCodeManager.ScanCodeAsync(
            string.IsNullOrWhiteSpace(input.Type) ? "QR_CODE" : input.Type.Trim(),
            input.Content);
        return MapToDto(entity);
    }

    private static ScanCodeAdminDto MapToDto(ScanCode entity)
    {
        return new ScanCodeAdminDto
        {
            Id = entity.Id,
            Type = entity.Type,
            Content = entity.Content,
            DeviceId = entity.DeviceId,
            UserId = entity.UserId,
            ClientId = entity.ClientId,
            Execution = entity.Execution,
            HandlerCount = entity.HandlerCount,
            CreationTime = entity.CreationTime,
            ScanHandlerList = entity.ScanHandlerList?.Select(h => new ScanCodeAdminHandlerDto
            {
                Id = h.Id,
                Action = h.Action,
                Handler = h.Handler,
                HandlerFullName = h.HandlerFullName,
                Execution = h.Execution,
                Message = h.Message,
                Data = h.Result,
                IsSuccess = h.Success
            }).ToList() ?? []
        };
    }
}
