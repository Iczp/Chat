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
        CancellationToken cancellationToken = default);

    Task<SessionAnnouncement> UpdateAsync(
        SessionAnnouncement announcement,
        string title,
        string content,
        EditorTypes editorType,
        bool isPinned,
        bool isPopup,
        bool requireConfirm,
        CancellationToken cancellationToken = default);

    Task SetPinnedAsync(
        SessionAnnouncement announcement,
        bool isPinned,
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
