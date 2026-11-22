using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class SetPinnedInput
{
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    public virtual bool IsPinned { get; set; }
}
