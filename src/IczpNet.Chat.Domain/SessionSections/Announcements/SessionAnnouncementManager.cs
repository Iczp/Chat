using IczpNet.Chat.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.Announcements;

public class SessionAnnouncementManager(
    ISessionAnnouncementRepository announcementRepository,
    ISessionAnnouncementReceiptRepository receiptRepository) : DomainService, ISessionAnnouncementManager
{
    protected ISessionAnnouncementRepository AnnouncementRepository { get; } = announcementRepository;
    protected ISessionAnnouncementReceiptRepository ReceiptRepository { get; } = receiptRepository;

    public virtual async Task<SessionAnnouncement> CreateAsync(
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
        CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(title, nameof(title));
        Check.NotNullOrWhiteSpace(content, nameof(content));

        var id = GuidGenerator.Create();
        var now = Clock.Now;

        if (isPinned)
        {
            await AnnouncementRepository.UnpinOthersAsync(sessionId, id, cancellationToken);
        }

        var announcement = new SessionAnnouncement(
            id: id,
            sessionId: sessionId,
            chatObjectId: chatObjectId,
            publisherSessionUnitId: publisherSessionUnitId,
            publisherChatObjectId: publisherChatObjectId,
            title: title.Trim(),
            content: content,
            editorType: editorType,
            status: AnnouncementStatus.Published,
            isPinned: isPinned,
            isPopup: isPopup,
            requireConfirm: requireConfirm,
            pinnedTime: isPinned ? now : null);

        return await AnnouncementRepository.InsertAsync(announcement, autoSave: true, cancellationToken: cancellationToken);
    }

    public virtual async Task<SessionAnnouncement> UpdateAsync(
        SessionAnnouncement announcement,
        string title,
        string content,
        EditorTypes editorType,
        bool isPinned,
        bool isPopup,
        bool requireConfirm,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(announcement, nameof(announcement));
        Check.NotNullOrWhiteSpace(title, nameof(title));
        Check.NotNullOrWhiteSpace(content, nameof(content));

        announcement.SetContent(title.Trim(), content, editorType);

        if (isPinned && !announcement.IsPinned && announcement.SessionId.HasValue)
        {
            await AnnouncementRepository.UnpinOthersAsync(announcement.SessionId.Value, announcement.Id, cancellationToken);
            announcement.SetPinned(true, Clock.Now);
        }
        else if (!isPinned && announcement.IsPinned)
        {
            announcement.SetPinned(false);
        }

        return await AnnouncementRepository.UpdateAsync(announcement, autoSave: true, cancellationToken: cancellationToken);
    }

    public virtual async Task SetPinnedAsync(
        SessionAnnouncement announcement,
        bool isPinned,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(announcement, nameof(announcement));

        if (isPinned && announcement.SessionId.HasValue)
        {
            await AnnouncementRepository.UnpinOthersAsync(announcement.SessionId.Value, announcement.Id, cancellationToken);
            announcement.SetPinned(true, Clock.Now);
        }
        else
        {
            announcement.SetPinned(false);
        }

        await AnnouncementRepository.UpdateAsync(announcement, autoSave: true, cancellationToken: cancellationToken);
    }

    public virtual async Task DeleteAsync(
        SessionAnnouncement announcement,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(announcement, nameof(announcement));
        await AnnouncementRepository.DeleteAsync(announcement, autoSave: true, cancellationToken: cancellationToken);
    }

    public virtual async Task<SessionAnnouncementReceipt> ConfirmReceiptAsync(
        Guid announcementId,
        Guid sessionUnitId,
        long chatObjectId,
        string deviceId = null,
        CancellationToken cancellationToken = default)
    {
        var announcement = await AnnouncementRepository.GetAsync(announcementId, cancellationToken: cancellationToken);
        var existing = await ReceiptRepository.FindByUnitAsync(announcementId, sessionUnitId, cancellationToken);
        var now = Clock.Now;

        if (existing != null)
        {
            if (!existing.IsConfirmed)
            {
                existing.Confirm(now, deviceId);
                await ReceiptRepository.UpdateAsync(existing, autoSave: true, cancellationToken: cancellationToken);
                announcement.IncrementConfirmedCount();
                await AnnouncementRepository.UpdateAsync(announcement, autoSave: true, cancellationToken: cancellationToken);
            }
            return existing;
        }

        var receipt = new SessionAnnouncementReceipt(announcementId, sessionUnitId, chatObjectId, now, deviceId);
        receipt.Confirm(now, deviceId);
        receipt = await ReceiptRepository.InsertAsync(receipt, autoSave: true, cancellationToken: cancellationToken);

        announcement.IncrementConfirmedCount();
        announcement.IncrementReadCount();
        await AnnouncementRepository.UpdateAsync(announcement, autoSave: true, cancellationToken: cancellationToken);

        return receipt;
    }

    public virtual async Task IncrementReadCountAsync(
        Guid announcementId,
        CancellationToken cancellationToken = default)
    {
        var announcement = await AnnouncementRepository.FindAsync(announcementId, cancellationToken: cancellationToken);
        if (announcement != null)
        {
            announcement.IncrementReadCount();
            await AnnouncementRepository.UpdateAsync(announcement, autoSave: true, cancellationToken: cancellationToken);
        }
    }
}
