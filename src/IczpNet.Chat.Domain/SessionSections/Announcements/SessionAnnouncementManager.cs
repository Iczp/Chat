using IczpNet.Chat.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionSections.Announcements;

public class SessionAnnouncementManager(
    ISessionAnnouncementRepository announcementRepository,
    ISessionAnnouncementReceiptRepository receiptRepository,
    IAbpDistributedLock distributedLock,
    IUnitOfWorkManager unitOfWorkManager,
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
        double? sorting = null,
        DateTime? expireTime = null,
        bool applyToNewMembers = false,
        AnnouncementStatus status = AnnouncementStatus.Published,
        DateTime? publishTime = null,
        CancellationToken cancellationToken = default)
    {
        Check.NotNullOrWhiteSpace(title, nameof(title));
        Check.NotNullOrWhiteSpace(content, nameof(content));

        var id = GenerateGuid();
        var now = CurrentTime;
        var effectiveSorting = sorting.HasValue
            ? (sorting.Value > 0 ? sorting.Value : 0)
            : (isPinned ? 1d : 0d);
        var effectivePinned = effectiveSorting > 0;

        var announcement = new SessionAnnouncement(
            id: id,
            sessionId: sessionId,
            chatObjectId: chatObjectId,
            publisherSessionUnitId: publisherSessionUnitId,
            publisherChatObjectId: publisherChatObjectId,
            title: title.Trim(),
            content: content,
            editorType: editorType,
            status: status,
            isPinned: effectivePinned,
            isPopup: isPopup,
            requireConfirm: requireConfirm,
            pinnedTime: effectivePinned ? now : null,
            sorting: effectiveSorting,
            expireTime: expireTime,
            applyToNewMembers: applyToNewMembers,
            publishTime: publishTime);

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
        double? sorting = null,
        DateTime? expireTime = null,
        bool applyToNewMembers = false,
        AnnouncementStatus? status = null,
        DateTime? publishTime = null,
        bool resetReceipts = false,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(announcement, nameof(announcement));
        Check.NotNullOrWhiteSpace(title, nameof(title));
        Check.NotNullOrWhiteSpace(content, nameof(content));

        announcement.SetContent(title.Trim(), content, editorType);
        announcement.SetOptions(isPopup, requireConfirm);
        announcement.SetPolicy(expireTime, applyToNewMembers, publishTime);
        announcement.SetPinned(isPinned, CurrentTime, sorting);
        if (status.HasValue)
        {
            announcement.SetStatus(status.Value);
        }

        if (resetReceipts)
        {
            await ReceiptRepository.ResetConfirmationsAsync(announcement.Id, cancellationToken);
            announcement.ResetConfirmedCount();
        }

        return await AnnouncementRepository.UpdateAsync(announcement, autoSave: true, cancellationToken: cancellationToken);
    }

    public virtual async Task PublishAsync(
        SessionAnnouncement announcement,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(announcement, nameof(announcement));
        announcement.SetStatus(AnnouncementStatus.Published);
        announcement.SetPolicy(announcement.ExpireTime, announcement.ApplyToNewMembers, null);
        await AnnouncementRepository.UpdateAsync(announcement, autoSave: true, cancellationToken: cancellationToken);
    }

    public virtual async Task RevokeAsync(
        SessionAnnouncement announcement,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(announcement, nameof(announcement));
        announcement.SetStatus(AnnouncementStatus.Revoked);
        await AnnouncementRepository.UpdateAsync(announcement, autoSave: true, cancellationToken: cancellationToken);
    }

    public virtual async Task SetPinnedAsync(
        SessionAnnouncement announcement,
        bool isPinned,
        double? sorting = null,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(announcement, nameof(announcement));

        announcement.SetPinned(isPinned, CurrentTime, sorting);

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
        return await SaveReceiptAsync(announcementId, sessionUnitId, chatObjectId, deviceId, confirm: true, cancellationToken);
    }

    public virtual async Task<SessionAnnouncementReceipt> RecordViewAsync(
        Guid announcementId,
        Guid sessionUnitId,
        long chatObjectId,
        string deviceId = null,
        CancellationToken cancellationToken = default)
    {
        return await SaveReceiptAsync(announcementId, sessionUnitId, chatObjectId, deviceId, confirm: false, cancellationToken);
    }

    private async Task<SessionAnnouncementReceipt> SaveReceiptAsync(Guid announcementId, Guid sessionUnitId,
        long chatObjectId, string deviceId, bool confirm, CancellationToken cancellationToken)
    {
        if (distributedLock is NullAbpDistributedLock)
            throw new InvalidOperationException("Announcement receipts require a distributed lock provider.");
        // Hold the per-member lock until commit; different members can proceed concurrently.
        await using var handle = await distributedLock.TryAcquireAsync(
            $"Chat:Announcement:Receipt:{announcementId:N}:{sessionUnitId:N}", TimeSpan.FromSeconds(10), cancellationToken);
        if (handle == null) throw new UserFriendlyException("公告回执正在处理中，请重试。");
        using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var announcement = await AnnouncementRepository.GetAsync(announcementId, cancellationToken: cancellationToken);
        if (announcement.Status != AnnouncementStatus.Published)
            throw new UserFriendlyException("公告尚未发布或已撤下。");
        var existing = await ReceiptRepository.FindByUnitAsync(announcementId, sessionUnitId, cancellationToken);
        var now = CurrentTime;
        var receipt = existing ?? new SessionAnnouncementReceipt(announcementId, sessionUnitId, chatObjectId, now, deviceId);
        var newConfirmation = confirm && !receipt.IsConfirmed;
        if (newConfirmation) receipt.Confirm(now, deviceId);
        if (existing == null)
            receipt = await ReceiptRepository.InsertAsync(receipt, autoSave: true, cancellationToken: cancellationToken);
        else if (newConfirmation)
            await ReceiptRepository.UpdateAsync(receipt, autoSave: true, cancellationToken: cancellationToken);
        if (existing == null || newConfirmation)
            await AnnouncementRepository.IncrementReceiptCountsAsync(announcementId, existing == null ? 1 : 0,
                newConfirmation ? 1 : 0, cancellationToken);
        await uow.CompleteAsync(cancellationToken);
        return receipt;
    }

    public virtual async Task IncrementReadCountAsync(
        Guid announcementId,
        CancellationToken cancellationToken = default)
    {
        await AnnouncementRepository.IncrementReceiptCountsAsync(announcementId, 1, 0, cancellationToken);
    }
}
