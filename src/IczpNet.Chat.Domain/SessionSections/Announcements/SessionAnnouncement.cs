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
[Index(nameof(Status), nameof(PublishTime))]
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

    [Comment("定时发布时间（null=立即发布）")]
    public virtual DateTime? PublishTime { get; protected set; }

    [Comment("是否对发布后新入群/重入群成员生效（弹窗/需确认）")]
    public virtual bool ApplyToNewMembers { get; protected set; }

    public virtual bool IsPopup { get; protected set; }

    public virtual bool RequireConfirm { get; protected set; }

    public virtual int ConfirmedCount { get; protected set; }

    public virtual int ReadCount { get; protected set; }

    public virtual long? LinkedMessageId { get; protected set; }

    [Comment("当前公告版本号（未发布=0，首次发布=1，升版递增）")]
    public virtual int Version { get; protected set; }

    [Comment("是否已通过审核（具备直发权或审核通过为 true）")]
    public virtual bool IsApproved { get; protected set; }

    public virtual Guid? ReviewerSessionUnitId { get; protected set; }

    [ForeignKey(nameof(ReviewerSessionUnitId))]
    public virtual SessionUnit ReviewerSessionUnit { get; protected set; }

    public virtual long? ReviewerChatObjectId { get; protected set; }

    [ForeignKey(nameof(ReviewerChatObjectId))]
    public virtual ChatObject ReviewerChatObject { get; protected set; }

    public virtual DateTime? ReviewedTime { get; protected set; }

    [StringLength(500)]
    public virtual string ReviewRemark { get; protected set; }

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
        bool applyToNewMembers = false,
        DateTime? publishTime = null) : base(id)
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
        PublishTime = publishTime;
        IsApproved = status == AnnouncementStatus.Published;
        Version = status == AnnouncementStatus.Published ? 1 : 0;
    }

    public virtual void SetContent(string title, string content, EditorTypes editorType)
    {
        Title = title;
        Content = content;
        EditorType = editorType;
        // ConcurrencyStamp 由 AbpDbContext 自动管理
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
        // ConcurrencyStamp 由 AbpDbContext 自动管理
    }

    public virtual void SetPolicy(DateTime? expireTime, bool applyToNewMembers, DateTime? publishTime = null)
    {
        ExpireTime = expireTime;
        ApplyToNewMembers = applyToNewMembers;
        PublishTime = publishTime;
        // ConcurrencyStamp 由 AbpDbContext 自动管理
    }

    public virtual void ResetConfirmedCount()
    {
        ConfirmedCount = 0;
        // ConcurrencyStamp 由 AbpDbContext 自动管理
    }

    public virtual void SetStatus(AnnouncementStatus status)
    {
        Status = status;
        if (status == AnnouncementStatus.Revoked || status == AnnouncementStatus.Rejected)
        {
            IsPinned = false;
            Sorting = 0;
            PinnedTime = null;
        }
        // ConcurrencyStamp 由 AbpDbContext 自动管理
    }

    public virtual void EnsureInitialPublishedVersion()
    {
        if (Version <= 0)
        {
            Version = 1;
        }
        IsApproved = true;
        // ConcurrencyStamp 由 AbpDbContext 自动管理
    }

    public virtual int IncrementVersion()
    {
        Version = Version <= 0 ? 1 : Version + 1;
        // ConcurrencyStamp 由 AbpDbContext 自动管理
        return Version;
    }

    public virtual void SetApproved(bool isApproved)
    {
        IsApproved = isApproved;
        // ConcurrencyStamp 由 AbpDbContext 自动管理
    }

    public virtual void SubmitForReview()
    {
        Status = AnnouncementStatus.PendingReview;
        IsApproved = false;
        ReviewerSessionUnitId = null;
        ReviewerChatObjectId = null;
        ReviewedTime = null;
        ReviewRemark = null;
        // ConcurrencyStamp 由 AbpDbContext 自动管理
    }

    public virtual void ApplyReview(
        bool approved,
        Guid reviewerSessionUnitId,
        long reviewerChatObjectId,
        DateTime reviewedTime,
        string remark,
        AnnouncementStatus approvedStatus)
    {
        ReviewerSessionUnitId = reviewerSessionUnitId;
        ReviewerChatObjectId = reviewerChatObjectId;
        ReviewedTime = reviewedTime;
        ReviewRemark = string.IsNullOrWhiteSpace(remark) ? null : remark.Trim();

        if (approved)
        {
            IsApproved = true;
            Status = approvedStatus;
            if (approvedStatus == AnnouncementStatus.Published)
            {
                if (Version <= 0)
                {
                    Version = 1;
                }
                PublishTime = null;
            }
        }
        else
        {
            IsApproved = false;
            SetStatus(AnnouncementStatus.Rejected);
        }

        // ConcurrencyStamp 由 AbpDbContext 自动管理
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
