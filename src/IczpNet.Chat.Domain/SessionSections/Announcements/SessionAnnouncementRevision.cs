using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.Announcements;

[Index(nameof(AnnouncementId), nameof(Version), AllDescending = true)]
public class SessionAnnouncementRevision : BaseEntity<Guid>
{
    public virtual Guid AnnouncementId { get; protected set; }

    [ForeignKey(nameof(AnnouncementId))]
    public virtual SessionAnnouncement Announcement { get; protected set; }

    public virtual int Version { get; protected set; }

    [Required]
    [StringLength(128)]
    public virtual string Title { get; protected set; }

    [Required]
    [MaxLength(10000)]
    public virtual string Content { get; protected set; }

    public virtual EditorTypes EditorType { get; protected set; }

    public virtual Guid EditorSessionUnitId { get; protected set; }

    [ForeignKey(nameof(EditorSessionUnitId))]
    public virtual SessionUnit EditorSessionUnit { get; protected set; }

    public virtual long EditorChatObjectId { get; protected set; }

    [ForeignKey(nameof(EditorChatObjectId))]
    public virtual ChatObject EditorChatObject { get; protected set; }

    [StringLength(300)]
    public virtual string ChangeSummary { get; protected set; }

    protected SessionAnnouncementRevision() { }

    public SessionAnnouncementRevision(
        Guid id,
        Guid announcementId,
        int version,
        string title,
        string content,
        EditorTypes editorType,
        Guid editorSessionUnitId,
        long editorChatObjectId,
        string changeSummary = null) : base(id)
    {
        AnnouncementId = announcementId;
        Version = version;
        Title = title;
        Content = content;
        EditorType = editorType;
        EditorSessionUnitId = editorSessionUnitId;
        EditorChatObjectId = editorChatObjectId;
        ChangeSummary = string.IsNullOrWhiteSpace(changeSummary) ? null : changeSummary.Trim();
    }
}
