using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.Sessions;

/// <summary>
/// 会话档案管理服务
/// </summary>
[Authorize(ChatAdminPermissions.Sessions.Default)]
[ExposeServices(typeof(SessionAppService), typeof(ISessionAdminAppService))]
public class SessionAppService(
    ISessionAdminManager sessionAdminManager,
    IChatObjectManager chatObjectManager,
    ISessionUnitRepository sessionUnitRepository)
    : ApplicationService, ISessionAdminAppService
{
    protected ISessionAdminManager SessionAdminManager { get; } = sessionAdminManager;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;

    [Authorize(ChatAdminPermissions.Sessions.GetList)]
    public virtual async Task<PagedResultDto<SessionAdminDto>> GetListAsync(
        SessionAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new SessionAdminGetListInput();

        var result = await SessionAdminManager.GetAdminListAsync(
            input.Keyword,
            input.OwnerId,
            input.Channel,
            input.IsEnabled,
            input.IsAllMuted,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        return new PagedResultDto<SessionAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.Sessions.GetDetail)]
    public virtual async Task<SessionAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await SessionAdminManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.Sessions.SetIsEnabled)]
    public virtual async Task<SessionAdminDto> SetIsEnabledAsync(
        Guid id,
        SessionAdminSetIsEnabledInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await SessionAdminManager.SetIsEnabledAdminAsync(id, input.IsEnabled, cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.Sessions.SetIsAllMuted)]
    public virtual async Task<SessionAdminDto> SetIsAllMutedAsync(
        Guid id,
        SessionAdminSetIsAllMutedInput input,
        CancellationToken cancellationToken = default)
    {
        var allowedSessionIds = Array.Empty<Guid>();
        if (CurrentUser.Id.HasValue)
        {
            var ownerIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
            if (ownerIds != null && ownerIds.Count > 0)
            {
                allowedSessionIds = (await AsyncExecuter.ToListAsync(
                    (await SessionUnitRepository.GetQueryableAsync())
                        .Where(x => ownerIds.Contains(x.OwnerId) && x.SessionId.HasValue && x.Setting.IsEnabled && !x.Setting.IsKilled)
                        .Select(x => x.SessionId!.Value)
                        .Distinct(),
                    cancellationToken)).ToArray();
            }
        }

        var scope = new SessionAdminScope
        {
            TenantId = CurrentTenant.Id,
            HasManageAll = false,
            AllowedSessionIds = allowedSessionIds
        };

        var entity = await SessionAdminManager.SetIsAllMutedAdminAsync(id, input.IsAllMuted, scope, cancellationToken);
        return MapToDto(entity);
    }

    private static SessionAdminDto MapToDto(Session entity)
    {
        return new SessionAdminDto
        {
            Id = entity.Id,
            SessionKey = entity.SessionKey,
            Channel = entity.Channel,
            Title = entity.Title,
            Description = entity.Description,
            OwnerId = entity.OwnerId,
            OwnerName = entity.Owner?.Name,
            IsEnabled = entity.IsEnabled,
            IsAllMuted = entity.IsAllMuted,
            FirstMessageId = entity.FirstMessageId,
            LastMessageId = entity.LastMessageId,
            LastMessageSeq = entity.LastMessageSeq,
            MessageTotalCount = entity.MessageTotalCount,
            CreationTime = entity.CreationTime
        };
    }
}
