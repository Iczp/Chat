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
[Index(nameof(SessionId), nameof(Sorting), AllDescending = true)]
[Index(nameof(SessionId), nameof(CreationTime), AllDescending = true)]
[Index(nameof(ExpireTime))]
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

    [Comment("置顶排序权重（0=不置顶，数值越大越置顶）")]
    public virtual double Sorting { get; protected set; }

    public virtual DateTime? PinnedTime { get; protected set; }

    [Comment("公告有效期截止时间（null=长期有效）")]
    public virtual DateTime? ExpireTime { get; protected set; }

    [Comment("是否对发布后新入群/重入群成员生效（弹窗/需确认）")]
    public virtual bool ApplyToNewMembers { get; protected set; }

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
        DateTime? pinnedTime = null,
        double sorting = 0,
        DateTime? expireTime = null,
        bool applyToNewMembers = false) : base(id)
    {
        SessionId = sessionId;
        ChatObjectId = chatObjectId;
        PublisherSessionUnitId = publisherSessionUnitId;
        PublisherChatObjectId = publisherChatObjectId;
        Title = title;
        Content = content;
        EditorType = editorType;
        Status = status;
        if (sorting > 0)
        {
            Sorting = sorting;
            IsPinned = true;
        }
        else
        {
            IsPinned = isPinned;
            Sorting = isPinned ? 1 : 0;
        }
        IsPopup = isPopup;
        RequireConfirm = requireConfirm;
        PinnedTime = IsPinned ? (pinnedTime ?? DateTime.Now) : null;
        ExpireTime = expireTime;
        ApplyToNewMembers = applyToNewMembers;
    }

    public virtual void SetContent(string title, string content, EditorTypes editorType)
    {
        Title = title;
        Content = content;
        EditorType = editorType;
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    public virtual void SetPinned(bool isPinned, DateTime? pinnedTime = null, double? sorting = null)
    {
        if (sorting.HasValue)
        {
            Sorting = sorting.Value > 0 ? sorting.Value : 0;
            IsPinned = Sorting > 0;
        }
        else
        {
            IsPinned = isPinned;
            Sorting = isPinned ? (Sorting > 0 ? Sorting : 1) : 0;
        }
        PinnedTime = IsPinned ? (pinnedTime ?? DateTime.Now) : null;
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    public virtual void SetPolicy(DateTime? expireTime, bool applyToNewMembers)
    {
        ExpireTime = expireTime;
        ApplyToNewMembers = applyToNewMembers;
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    public virtual void SetStatus(AnnouncementStatus status)
    {
        Status = status;
        if (status == AnnouncementStatus.Revoked)
        {
            IsPinned = false;
            Sorting = 0;
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
