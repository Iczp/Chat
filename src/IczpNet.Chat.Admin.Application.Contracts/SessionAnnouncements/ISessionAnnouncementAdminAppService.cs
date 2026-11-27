using System;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.SessionAnnouncements;

public class SessionAnnouncementAdminGetListInput : PagedResultRequestDto
{
    public Guid? SessionId { get; set; }

    public string Keyword { get; set; }

    public AnnouncementStatus? Status { get; set; }

    public bool? IsPinned { get; set; }
}

public class SessionAnnouncementAdminDto : EntityDto<Guid>
{
    public Guid? SessionId { get; set; }

    public string SessionTitle { get; set; }

    public long ChatObjectId { get; set; }

    public long PublisherChatObjectId { get; set; }

    public string PublisherName { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public AnnouncementStatus Status { get; set; }

    public bool IsPinned { get; set; }

    public bool IsPopup { get; set; }

    public bool RequireConfirm { get; set; }

    public int ConfirmedCount { get; set; }

    public int ReadCount { get; set; }

    public int Version { get; set; }

    public bool IsApproved { get; set; }

    public DateTime? PublishTime { get; set; }

    public DateTime? ExpireTime { get; set; }

    public DateTime CreationTime { get; set; }
}

public class SessionAnnouncementAdminSetPinnedInput
{
    public bool IsPinned { get; set; }
}

public interface ISessionAnnouncementAdminAppService : IApplicationService
{
    Task<PagedResultDto<SessionAnnouncementAdminDto>> GetListAsync(
        SessionAnnouncementAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<SessionAnnouncementAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SessionAnnouncementAdminDto> SetPinnedAsync(
        Guid id,
        SessionAnnouncementAdminSetPinnedInput input,
        CancellationToken cancellationToken = default);
}
