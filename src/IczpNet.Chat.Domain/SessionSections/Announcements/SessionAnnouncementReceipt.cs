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

    public virtual int ReadVersion { get; protected set; }

    public virtual int ConfirmedVersion { get; protected set; }

    protected SessionAnnouncementReceipt() { }

    public SessionAnnouncementReceipt(
        Guid announcementId,
        Guid sessionUnitId,
        long chatObjectId,
        DateTime firstReadTime,
        string deviceId = null,
        int readVersion = 1)
    {
        AnnouncementId = announcementId;
        SessionUnitId = sessionUnitId;
        ChatObjectId = chatObjectId;
        FirstReadTime = firstReadTime;
        DeviceId = deviceId;
        IsConfirmed = false;
        ReadVersion = readVersion > 0 ? readVersion : 1;
        ConfirmedVersion = 0;
    }

    public virtual bool MarkReadVersion(int version)
    {
        if (version > ReadVersion)
        {
            ReadVersion = version;
            return true;
        }
        return false;
    }

    public virtual void Confirm(DateTime confirmedTime, string deviceId = null, int confirmedVersion = 1)
    {
        var targetVersion = confirmedVersion > 0 ? confirmedVersion : 1;
        if (IsConfirmed && ConfirmedVersion >= targetVersion) return;

        var firstTimeConfirm = !IsConfirmed;
        IsConfirmed = true;
        ConfirmedTime = confirmedTime;
        if (targetVersion > ConfirmedVersion)
        {
            ConfirmedVersion = targetVersion;
        }
        if (targetVersion > ReadVersion)
        {
            ReadVersion = targetVersion;
        }
        if (firstTimeConfirm && !string.IsNullOrEmpty(deviceId))
        {
            DeviceId = deviceId;
        }
        else if (string.IsNullOrEmpty(DeviceId) && !string.IsNullOrEmpty(deviceId))
        {
            DeviceId = deviceId;
        }
    }
}
