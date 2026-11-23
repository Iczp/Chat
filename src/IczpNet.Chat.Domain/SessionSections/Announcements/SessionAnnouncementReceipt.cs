using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.SessionSections.Announcements;

[Index(nameof(AnnouncementId), nameof(SessionUnitId), IsUnique = true)]
[Index(nameof(AnnouncementId), nameof(IsConfirmed))]
public class SessionAnnouncementReceipt : Entity<long>
{
    public virtual Guid AnnouncementId { get; protected set; }

    [ForeignKey(nameof(AnnouncementId))]
    public virtual SessionAnnouncement Announcement { get; protected set; }

    public virtual Guid SessionUnitId { get; protected set; }

    [ForeignKey(nameof(SessionUnitId))]
    public virtual SessionUnit SessionUnit { get; protected set; }

    public virtual long ChatObjectId { get; protected set; }

    [ForeignKey(nameof(ChatObjectId))]
    public virtual ChatObject ChatObject { get; protected set; }

    public virtual DateTime FirstReadTime { get; protected set; }

    public virtual bool IsConfirmed { get; protected set; }

    public virtual DateTime? ConfirmedTime { get; protected set; }

    [StringLength(64)]
    public virtual string DeviceId { get; protected set; }

    protected SessionAnnouncementReceipt() { }

    public SessionAnnouncementReceipt(
        Guid announcementId,
        Guid sessionUnitId,
        long chatObjectId,
        DateTime firstReadTime,
        string deviceId = null)
    {
        AnnouncementId = announcementId;
        SessionUnitId = sessionUnitId;
        ChatObjectId = chatObjectId;
        FirstReadTime = firstReadTime;
        DeviceId = deviceId;
        IsConfirmed = false;
    }

    public virtual void Confirm(DateTime confirmedTime, string deviceId = null)
    {
        if (IsConfirmed) return;
        IsConfirmed = true;
        ConfirmedTime = confirmedTime;
        if (!string.IsNullOrEmpty(deviceId))
        {
            DeviceId = deviceId;
        }
    }
}
