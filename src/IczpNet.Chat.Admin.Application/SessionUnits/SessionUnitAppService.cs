using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ChatObjects.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.Admin.SessionUnits.Dtos;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.SessionUnits;

/// <summary>
/// 会话单元管理查询服务
/// </summary>
[Authorize(ChatAdminPermissions.SessionUnits.Default)]
[ExposeServices(typeof(SessionUnitAppService), typeof(ISessionUnitAdminAppService))]
public class SessionUnitAppService(ISessionUnitAdminManager sessionUnitAdminManager)
    : ApplicationService, ISessionUnitAdminAppService
{
    protected ISessionUnitAdminManager SessionUnitAdminManager { get; } = sessionUnitAdminManager;

    [Authorize(ChatAdminPermissions.SessionUnits.GetList)]
    public virtual async Task<PagedResultDto<SessionUnitAdminDto>> GetListAsync(
        SessionUnitAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new SessionUnitAdminGetListInput();

        var result = await SessionUnitAdminManager.GetAdminListAsync(
            input.OwnerId,
            input.DestinationId,
            input.DestinationObjectType,
            input.IsKilled,
            input.Keyword,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        return new PagedResultDto<SessionUnitAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.SessionUnits.GetDetail)]
    public virtual async Task<SessionUnitAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await SessionUnitAdminManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.SessionUnits.GetList)]
    public virtual Task<Guid> FindIdAsync(
        [Required] long ownerId,
        [Required] long destinactionId,
        CancellationToken cancellationToken = default)
    {
        return SessionUnitAdminManager.FindIdAdminAsync(ownerId, destinactionId, cancellationToken);
    }

    private static SessionUnitAdminDto MapToDto(SessionUnit entity)
    {
        return new SessionUnitAdminDto
        {
            Id = entity.Id,
            OwnerId = entity.OwnerId,
            DestinationId = entity.DestinationId,
            DestinationObjectType = entity.DestinationObjectType,
            SessionId = entity.SessionId,
            Destination = entity.Destination == null ? null : MapDestinationDto(entity.Destination),
            Setting = entity.Setting == null
                ? null
                : new SessionUnitAdminSettingDto
                {
                    IsEnabled = entity.Setting.IsEnabled,
                    IsInputEnabled = entity.Setting.IsInputEnabled,
                    IsKilled = entity.Setting.IsKilled
                },
            LastMessageId = entity.LastMessageId,
            PublicBadge = entity.PublicBadge,
            PrivateBadge = entity.PrivateBadge,
            CreationTime = entity.CreationTime
        };
    }

    private static ChatObjectAdminListItemDto MapDestinationDto(ChatObject entity)
    {
        return new ChatObjectAdminListItemDto
        {
            Id = entity.Id,
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            Code = entity.Code,
            AppUserId = entity.AppUserId,
            ChatObjectTypeId = entity.ChatObjectTypeId,
            ObjectType = entity.ObjectType,
            ObjectTypeDescription = entity.ObjectTypeDescription,
            Gender = entity.Gender,
            VerificationMethod = entity.VerificationMethod,
            Thumbnail = entity.Thumbnail,
            Portrait = entity.Portrait,
            Description = entity.Description,
            ParentId = entity.ParentId,
            Depth = entity.Depth,
            FullPath = entity.FullPath,
            FullPathName = entity.FullPathName,
            IsEnabled = entity.IsEnabled,
            IsDefault = entity.IsDefault,
            IsPublic = entity.IsPublic,
            IsStatic = entity.IsStatic,
            IsActive = entity.IsActive,
            IsDeveloper = entity.IsDeveloper,
            CreationTime = entity.CreationTime
        };
    }
}
