using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.Announcements;

[Index(nameof(SessionId), nameof(IsPinned))]
[Index(nameof(SessionId), nameof(CreationTime), AllDescending = true)]
[Index(nameof(ChatObjectId))]
public class SessionAnnouncement : BaseEntity<Guid>, ISessionId
{
    public virtual Guid? SessionId { get; protected set; }

    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; protected set; }

    public virtual long ChatObjectId { get; protected set; }

    [ForeignKey(nameof(ChatObjectId))]
    public virtual ChatObject ChatObject { get; protected set; }

    public virtual Guid PublisherSessionUnitId { get; protected set; }

    [ForeignKey(nameof(PublisherSessionUnitId))]
    public virtual SessionUnit PublisherSessionUnit { get; protected set; }

    public virtual long PublisherChatObjectId { get; protected set; }

    [ForeignKey(nameof(PublisherChatObjectId))]
    public virtual ChatObject PublisherChatObject { get; protected set; }

    [Required]
    [StringLength(128)]
    public virtual string Title { get; protected set; }

    [Required]
    [MaxLength(10000)]
    public virtual string Content { get; protected set; }

    public virtual EditorTypes EditorType { get; protected set; }

    public virtual AnnouncementStatus Status { get; protected set; }

    public virtual bool IsPinned { get; protected set; }

    public virtual DateTime? PinnedTime { get; protected set; }

    public virtual bool IsPopup { get; protected set; }

    public virtual bool RequireConfirm { get; protected set; }

    public virtual int ConfirmedCount { get; protected set; }

    public virtual int ReadCount { get; protected set; }

    public virtual long? LinkedMessageId { get; protected set; }

    protected SessionAnnouncement() { }

    public SessionAnnouncement(
        Guid id,
        Guid sessionId,
        long chatObjectId,
        Guid publisherSessionUnitId,
        long publisherChatObjectId,
        string title,
        string content,
        EditorTypes editorType = EditorTypes.None,
        AnnouncementStatus status = AnnouncementStatus.Published,
        bool isPinned = false,
        bool isPopup = false,
        bool requireConfirm = false,
        DateTime? pinnedTime = null) : base(id)
    {
        SessionId = sessionId;
        ChatObjectId = chatObjectId;
        PublisherSessionUnitId = publisherSessionUnitId;
        PublisherChatObjectId = publisherChatObjectId;
        Title = title;
        Content = content;
        EditorType = editorType;
        Status = status;
        IsPinned = isPinned;
        IsPopup = isPopup;
        RequireConfirm = requireConfirm;
        PinnedTime = isPinned ? (pinnedTime ?? DateTime.Now) : null;
    }

    public virtual void SetContent(string title, string content, EditorTypes editorType)
    {
        Title = title;
        Content = content;
        EditorType = editorType;
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    public virtual void SetPinned(bool isPinned, DateTime? pinnedTime = null)
    {
        IsPinned = isPinned;
        PinnedTime = isPinned ? (pinnedTime ?? DateTime.Now) : null;
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    public virtual void SetStatus(AnnouncementStatus status)
    {
        Status = status;
        if (status == AnnouncementStatus.Revoked)
        {
            IsPinned = false;
            PinnedTime = null;
        }
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    public virtual void SetOptions(bool isPopup, bool requireConfirm)
    {
        IsPopup = isPopup;
        RequireConfirm = requireConfirm;
    }

    public virtual void SetLinkedMessageId(long? messageId)
    {
        LinkedMessageId = messageId;
    }

    public virtual void IncrementConfirmedCount()
    {
        ConfirmedCount++;
    }

    public virtual void IncrementReadCount()
    {
        ReadCount++;
    }
}
