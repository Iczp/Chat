using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class GetSessionAnnouncementListInput : PagedAndSortedResultRequestDto
{
    [Required]
    public virtual Guid SessionId { get; set; }

    public virtual bool? IsPinned { get; set; }

    public virtual string Keyword { get; set; }

    public virtual bool IncludeDrafts { get; set; } = false;
}
