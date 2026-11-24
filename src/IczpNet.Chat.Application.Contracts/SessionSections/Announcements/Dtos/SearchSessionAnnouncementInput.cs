using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class SearchSessionAnnouncementInput : PagedAndSortedResultRequestDto
{
    [Required]
    [StringLength(128)]
    public virtual string Keyword { get; set; }

    public virtual Guid? SessionId { get; set; }
}
