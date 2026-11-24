using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public enum AnnouncementMemberFilterType : int
{
    All = 0,
    Read = 1,
    Unread = 2,
    Confirmed = 3,
    Unconfirmed = 4
}

public class GetAnnouncementMemberListInput : PagedAndSortedResultRequestDto
{
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    public virtual AnnouncementMemberFilterType FilterType { get; set; } = AnnouncementMemberFilterType.All;

    public virtual string Keyword { get; set; }

    /// <summary>
    /// 大群游标分页：上一页最后一条的 JoinTime
    /// </summary>
    public virtual DateTime? CursorJoinTime { get; set; }

    /// <summary>
    /// 大群游标分页：上一页最后一条的 SessionUnitId
    /// </summary>
    public virtual Guid? CursorSessionUnitId { get; set; }
}
