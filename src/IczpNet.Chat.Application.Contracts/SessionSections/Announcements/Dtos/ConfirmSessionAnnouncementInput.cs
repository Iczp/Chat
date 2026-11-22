using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class ConfirmSessionAnnouncementInput
{
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    public virtual string DeviceId { get; set; }
}
