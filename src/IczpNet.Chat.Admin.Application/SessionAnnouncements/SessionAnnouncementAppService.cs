using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.SessionSections.Announcements;
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
public class SessionAnnouncementAppService(ISessionAnnouncementAdminManager announcementAdminManager)
    : ApplicationService, ISessionAnnouncementAdminAppService
{
    protected ISessionAnnouncementAdminManager AnnouncementAdminManager { get; } = announcementAdminManager;

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
