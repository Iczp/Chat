using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.SessionRoles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.SessionRoles;

/// <summary>
/// 群角色管理服务
/// </summary>
[Authorize(ChatAdminPermissions.SessionRoles.Default)]
[ExposeServices(typeof(SessionRoleAppService), typeof(ISessionRoleAdminAppService))]
public class SessionRoleAppService(ISessionRoleAdminManager sessionRoleAdminManager)
    : ApplicationService, ISessionRoleAdminAppService
{
    protected ISessionRoleAdminManager SessionRoleAdminManager { get; } = sessionRoleAdminManager;

    [Authorize(ChatAdminPermissions.SessionRoles.GetList)]
    public virtual async Task<PagedResultDto<SessionRoleAdminDto>> GetListAsync(
        SessionRoleAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new SessionRoleAdminGetListInput();

        var result = await SessionRoleAdminManager.GetAdminListAsync(
            input.SessionId,
            input.Keyword,
            input.IsDefault,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        return new PagedResultDto<SessionRoleAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.SessionRoles.GetDetail)]
    public virtual async Task<SessionRoleAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await SessionRoleAdminManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDto(entity);
    }

    private static SessionRoleAdminDto MapToDto(SessionRole entity)
    {
        return new SessionRoleAdminDto
        {
            Id = entity.Id,
            SessionId = entity.SessionId,
            SessionTitle = entity.Session?.Title,
            Name = entity.Name,
            Description = entity.Description,
            IsDefault = entity.IsDefault,
            PermissionCount = entity.PermissionCount,
            CreationTime = entity.CreationTime
        };
    }
}
