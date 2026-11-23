using IczpNet.Chat.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;
using Volo.Abp.Timing;

namespace IczpNet.Chat.SessionSections.Announcements;

public class SessionAnnouncementManager(
    ISessionAnnouncementRepository announcementRepository,
    ISessionAnnouncementReceiptRepository receiptRepository,
    IGuidGenerator guidGenerator = null,
    IClock clock = null) : DomainService, ISessionAnnouncementManager
{
    protected ISessionAnnouncementRepository AnnouncementRepository { get; } = announcementRepository;
    protected ISessionAnnouncementReceiptRepository ReceiptRepository { get; } = receiptRepository;
    protected IGuidGenerator CustomGuidGenerator { get; } = guidGenerator;
    protected IClock CustomClock { get; } = clock;

    protected DateTime CurrentTime => (CustomClock ?? (LazyServiceProvider != null ? base.Clock : null))?.Now ?? DateTime.Now;
    protected Guid GenerateGuid() => (CustomGuidGenerator ?? (LazyServiceProvider != null ? base.GuidGenerator : null))?.Create() ?? Guid.NewGuid();

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

        var id = GenerateGuid();
        var now = CurrentTime;

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
            announcement.SetPinned(true, CurrentTime);
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
            announcement.SetPinned(true, CurrentTime);
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
        var now = CurrentTime;

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

    public virtual async Task<SessionAnnouncementReceipt> RecordViewAsync(
        Guid announcementId,
        Guid sessionUnitId,
        long chatObjectId,
        string deviceId = null,
        CancellationToken cancellationToken = default)
    {
        var announcement = await AnnouncementRepository.GetAsync(announcementId, cancellationToken: cancellationToken);
        var existing = await ReceiptRepository.FindByUnitAsync(announcementId, sessionUnitId, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        var now = CurrentTime;
        var receipt = new SessionAnnouncementReceipt(announcementId, sessionUnitId, chatObjectId, now, deviceId);
        receipt = await ReceiptRepository.InsertAsync(receipt, autoSave: true, cancellationToken: cancellationToken);

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
