using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.Announcements;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.SessionAnnouncements;

/// <summary>
/// 群公告管理服务
/// </summary>
[Authorize(ChatAdminPermissions.SessionAnnouncements.Default)]
[ExposeServices(typeof(SessionAnnouncementAppService), typeof(ISessionAnnouncementAdminAppService))]
public class SessionAnnouncementAppService(
    ISessionAnnouncementAdminManager announcementAdminManager,
    IChatObjectManager chatObjectManager,
    ISessionUnitRepository sessionUnitRepository)
    : ApplicationService, ISessionAnnouncementAdminAppService
{
    protected ISessionAnnouncementAdminManager AnnouncementAdminManager { get; } = announcementAdminManager;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;

    [Authorize(ChatAdminPermissions.SessionAnnouncements.GetList)]
    public virtual async Task<PagedResultDto<SessionAnnouncementAdminDto>> GetListAsync(
        SessionAnnouncementAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new SessionAnnouncementAdminGetListInput();

        var result = await AnnouncementAdminManager.GetAdminListAsync(
            input.SessionId,
            input.Keyword,
            input.Status,
            input.IsPinned,
            input.SkipCount,
            input.MaxResultCount,
            cancellationToken);

        return new PagedResultDto<SessionAnnouncementAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.SessionAnnouncements.GetDetail)]
    public virtual async Task<SessionAnnouncementAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await AnnouncementAdminManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.SessionAnnouncements.Pin)]
    public virtual async Task<SessionAnnouncementAdminDto> SetPinnedAsync(
        Guid id,
        SessionAnnouncementAdminSetPinnedInput input,
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

        var scope = new SessionAnnouncementAdminScope
        {
            TenantId = CurrentTenant.Id,
            HasManageAll = false,
            AllowedSessionIds = allowedSessionIds
        };

        var entity = await AnnouncementAdminManager.SetPinnedAdminAsync(id, input.IsPinned, scope, cancellationToken);
        return MapToDto(entity);
    }

    private static SessionAnnouncementAdminDto MapToDto(SessionAnnouncement entity)
    {
        return new SessionAnnouncementAdminDto
        {
            Id = entity.Id,
            SessionId = entity.SessionId,
            SessionTitle = entity.Session?.Title,
            ChatObjectId = entity.ChatObjectId,
            PublisherChatObjectId = entity.PublisherChatObjectId,
            PublisherName = entity.PublisherChatObject?.Name,
            Title = entity.Title,
            Content = entity.Content,
            Status = entity.Status,
            IsPinned = entity.IsPinned,
            IsPopup = entity.IsPopup,
            RequireConfirm = entity.RequireConfirm,
            ConfirmedCount = entity.ConfirmedCount,
            ReadCount = entity.ReadCount,
            Version = entity.Version,
            IsApproved = entity.IsApproved,
            PublishTime = entity.PublishTime,
            ExpireTime = entity.ExpireTime,
            CreationTime = entity.CreationTime
        };
    }
}
