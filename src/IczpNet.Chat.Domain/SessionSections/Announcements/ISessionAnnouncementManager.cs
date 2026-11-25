using IczpNet.Chat.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.Announcements;

public interface ISessionAnnouncementManager
{
    Task<SessionAnnouncement> CreateAsync(
        Guid sessionId,
        long chatObjectId,
        Guid publisherSessionUnitId,
        long publisherChatObjectId,
        string title,
        string content,
        EditorTypes editorType = EditorTypes.None,
        bool isPinned = false,
        bool isPopup = false,
        bool requireConfirm = false,
        double? sorting = null,
        DateTime? expireTime = null,
        bool applyToNewMembers = false,
        AnnouncementStatus status = AnnouncementStatus.Published,
        DateTime? publishTime = null,
        bool isApproved = true,
        CancellationToken cancellationToken = default);

    Task<SessionAnnouncement> UpdateAsync(
        SessionAnnouncement announcement,
        string title,
        string content,
        EditorTypes editorType,
        bool isPinned,
        bool isPopup,
        bool requireConfirm,
        double? sorting = null,
        DateTime? expireTime = null,
        bool applyToNewMembers = false,
        AnnouncementStatus? status = null,
        DateTime? publishTime = null,
        bool resetReceipts = false,
        bool bumpVersion = false,
        string changeSummary = null,
        Guid? editorSessionUnitId = null,
        long? editorChatObjectId = null,
        CancellationToken cancellationToken = default);

    Task PublishAsync(
        SessionAnnouncement announcement,
        CancellationToken cancellationToken = default);

    Task SubmitForReviewAsync(
        SessionAnnouncement announcement,
        CancellationToken cancellationToken = default);

    Task ReviewAsync(
        SessionAnnouncement announcement,
        bool approved,
        Guid reviewerSessionUnitId,
        long reviewerChatObjectId,
        string remark = null,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        SessionAnnouncement announcement,
        CancellationToken cancellationToken = default);

    Task SetPinnedAsync(
        SessionAnnouncement announcement,
        bool isPinned,
        double? sorting = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        SessionAnnouncement announcement,
        CancellationToken cancellationToken = default);

    Task<SessionAnnouncementReceipt> ConfirmReceiptAsync(
        Guid announcementId,
        Guid sessionUnitId,
        long chatObjectId,
        string deviceId = null,
        CancellationToken cancellationToken = default);

    Task<SessionAnnouncementReceipt> RecordViewAsync(
        Guid announcementId,
        Guid sessionUnitId,
        long chatObjectId,
        string deviceId = null,
        CancellationToken cancellationToken = default);

    Task IncrementReadCountAsync(
        Guid announcementId,
        CancellationToken cancellationToken = default);
}
