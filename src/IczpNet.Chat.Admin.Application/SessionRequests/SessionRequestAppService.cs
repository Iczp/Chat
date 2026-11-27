using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.SessionRequests;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.SessionRequests;

/// <summary>
/// 入群与好友申请管理服务
/// </summary>
[Authorize(ChatAdminPermissions.SessionRequests.Default)]
[ExposeServices(typeof(SessionRequestAppService), typeof(ISessionRequestAdminAppService))]
public class SessionRequestAppService(ISessionRequestAdminManager sessionRequestAdminManager)
    : ApplicationService, ISessionRequestAdminAppService
{
    protected ISessionRequestAdminManager SessionRequestAdminManager { get; } = sessionRequestAdminManager;

    [Authorize(ChatAdminPermissions.SessionRequests.GetList)]
    public virtual async Task<PagedResultDto<SessionRequestAdminDto>> GetListAsync(
        SessionRequestAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new SessionRequestAdminGetListInput();

        var result = await SessionRequestAdminManager.GetAdminListAsync(
            input.Keyword,
            input.OwnerId,
            input.DestinationId,
            input.IsHandled,
            input.IsAgreed,
            input.IsExpired,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        return new PagedResultDto<SessionRequestAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.SessionRequests.GetDetail)]
    public virtual async Task<SessionRequestAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await SessionRequestAdminManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDto(entity);
    }

    private static SessionRequestAdminDto MapToDto(SessionRequest entity)
    {
        return new SessionRequestAdminDto
        {
            Id = entity.Id,
            OwnerId = entity.OwnerId,
            OwnerName = entity.Owner?.Name,
            DestinationId = entity.DestinationId,
            DestinationName = entity.Destination?.Name,
            RequestMessage = entity.RequestMessage,
            IsHandled = entity.IsHandled,
            IsAgreed = entity.IsAgreed,
            HandleMessage = entity.HandleMessage,
            HandleTime = entity.HandleTime,
            IsEnabled = entity.IsEnabled,
            IsExpired = entity.IsExpired,
            ExpirationTime = entity.ExpirationTime,
            CreationTime = entity.CreationTime
        };
    }
}
