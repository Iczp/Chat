using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Announcements;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionSections;

public class SessionAnnouncementManager_Tests
{
    private readonly ISessionAnnouncementRepository _announcementRepository;
    private readonly ISessionAnnouncementReceiptRepository _receiptRepository;
    private readonly SessionAnnouncementManager _manager;
    private readonly IAbpDistributedLockHandle _handle;
    private readonly IUnitOfWork _uow;

    public SessionAnnouncementManager_Tests()
    {
        _announcementRepository = Substitute.For<ISessionAnnouncementRepository>();
        _receiptRepository = Substitute.For<ISessionAnnouncementReceiptRepository>();
        var distributedLock = Substitute.For<IAbpDistributedLock>();
        _handle = Substitute.For<IAbpDistributedLockHandle>();
        distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Returns(_handle);
        var uowManager = Substitute.For<IUnitOfWorkManager>();
        _uow = Substitute.For<IUnitOfWork>();
        uowManager.Begin(Arg.Any<AbpUnitOfWorkOptions>(), true).Returns(_uow);
        _manager = new SessionAnnouncementManager(_announcementRepository, _receiptRepository, distributedLock, uowManager);
    }

    [Fact]
    public async Task CreateAsync_Should_UnpinOthers_When_IsPinned_True()
    {
        var sessionId = Guid.NewGuid();
        var chatObjectId = 1001L;
        var publisherUnitId = Guid.NewGuid();
        var publisherChatObjectId = 2001L;

        _announcementRepository.InsertAsync(Arg.Any<SessionAnnouncement>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(x => x.Arg<SessionAnnouncement>());

        var announcement = await _manager.CreateAsync(
            sessionId,
            chatObjectId,
            publisherUnitId,
            publisherChatObjectId,
            "群公告标题",
            "公告正文内容",
            EditorTypes.None,
            isPinned: true);

        Assert.NotNull(announcement);
        Assert.Equal("群公告标题", announcement.Title);
        Assert.True(announcement.IsPinned);
        Assert.NotNull(announcement.PinnedTime);

        // 验证排他性 unpin 其他公告
        await _announcementRepository.Received(1).UnpinOthersAsync(sessionId, announcement.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetPinnedAsync_Should_Toggle_Pinned_Status()
    {
        var sessionId = Guid.NewGuid();
        var announcement = new SessionAnnouncement(
            Guid.NewGuid(), sessionId, 1001L, Guid.NewGuid(), 2001L,
            "标题", "内容", isPinned: false);

        await _manager.SetPinnedAsync(announcement, true);

        Assert.True(announcement.IsPinned);
        Assert.NotNull(announcement.PinnedTime);
        await _announcementRepository.Received(1).UnpinOthersAsync(sessionId, announcement.Id, Arg.Any<CancellationToken>());

        // 再次取消置顶
        await _manager.SetPinnedAsync(announcement, false);
        Assert.False(announcement.IsPinned);
        Assert.Null(announcement.PinnedTime);
    }

    [Fact]
    public async Task RecordViewAsync_Should_Insert_Receipt_And_Increment_ReadCount()
    {
        var announcementId = Guid.NewGuid();
        var sessionUnitId = Guid.NewGuid();
        var chatObjectId = 2002L;

        var announcement = new SessionAnnouncement(
            announcementId, Guid.NewGuid(), 1001L, Guid.NewGuid(), 2001L,
            "标题", "内容");

        _announcementRepository.GetAsync(announcementId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(announcement);
        _receiptRepository.FindByUnitAsync(announcementId, sessionUnitId, Arg.Any<CancellationToken>())
            .Returns((SessionAnnouncementReceipt)null);
        _receiptRepository.InsertAsync(Arg.Any<SessionAnnouncementReceipt>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(x => x.Arg<SessionAnnouncementReceipt>());

        var receipt = await _manager.RecordViewAsync(announcementId, sessionUnitId, chatObjectId, "device-1");

        Assert.NotNull(receipt);
        Assert.False(receipt.IsConfirmed);
        await _announcementRepository.Received(1).IncrementReceiptCountsAsync(announcementId, 1, 0, Arg.Any<CancellationToken>());

        await _receiptRepository.Received(1).InsertAsync(Arg.Any<SessionAnnouncementReceipt>(), true, Arg.Any<CancellationToken>());
        await _announcementRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default, default, default);
    }

    [Fact]
    public async Task ConfirmReceiptAsync_Should_Update_Status_And_Increment_ConfirmedCount()
    {
        var announcementId = Guid.NewGuid();
        var sessionUnitId = Guid.NewGuid();
        var chatObjectId = 2002L;

        var announcement = new SessionAnnouncement(
            announcementId, Guid.NewGuid(), 1001L, Guid.NewGuid(), 2001L,
            "标题", "内容");

        _announcementRepository.GetAsync(announcementId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(announcement);
        _receiptRepository.FindByUnitAsync(announcementId, sessionUnitId, Arg.Any<CancellationToken>())
            .Returns((SessionAnnouncementReceipt)null);
        _receiptRepository.InsertAsync(Arg.Any<SessionAnnouncementReceipt>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(x => x.Arg<SessionAnnouncementReceipt>());

        var receipt = await _manager.ConfirmReceiptAsync(announcementId, sessionUnitId, chatObjectId, "device-1");

        Assert.NotNull(receipt);
        Assert.True(receipt.IsConfirmed);
        Assert.NotNull(receipt.ConfirmedTime);
        await _announcementRepository.Received(1).IncrementReceiptCountsAsync(announcementId, 1, 1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_preserves_popup_and_confirmation_options()
    {
        var announcement = new SessionAnnouncement(Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(), 2, "title", "body");
        await _manager.UpdateAsync(announcement, "updated", "updated body", EditorTypes.None, false, true, true);
        Assert.True(announcement.IsPopup);
        Assert.True(announcement.RequireConfirm);
        await _manager.UpdateAsync(announcement, "updated", "updated body", EditorTypes.None, false, false, false);
        Assert.False(announcement.IsPopup);
        Assert.False(announcement.RequireConfirm);
    }

    [Fact]
    public async Task Duplicate_confirmation_does_not_change_time_or_increment_counts_and_lock_covers_commit()
    {
        var id = Guid.NewGuid();
        var member = Guid.NewGuid();
        var announcement = new SessionAnnouncement(id, Guid.NewGuid(), 1, Guid.NewGuid(), 2, "title", "body");
        var time = new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc);
        var receipt = new SessionAnnouncementReceipt(id, member, 2, time, "original");
        receipt.Confirm(time);
        _announcementRepository.GetAsync(id, cancellationToken: Arg.Any<CancellationToken>()).Returns(announcement);
        _receiptRepository.FindByUnitAsync(id, member, Arg.Any<CancellationToken>()).Returns(receipt);
        var committed = false;
        _uow.CompleteAsync(Arg.Any<CancellationToken>()).Returns(_ => { committed = true; return Task.CompletedTask; });
        _handle.DisposeAsync().Returns(_ => { Assert.True(committed); return ValueTask.CompletedTask; });
        Assert.Same(receipt, await _manager.ConfirmReceiptAsync(id, member, 2, "second-device"));
        Assert.Equal(time, receipt.ConfirmedTime);
        Assert.Equal("original", receipt.DeviceId);
        await _receiptRepository.DidNotReceiveWithAnyArgs().InsertAsync(default, default, default);
        await _receiptRepository.DidNotReceiveWithAnyArgs().UpdateAsync(default, default, default);
        await _announcementRepository.DidNotReceiveWithAnyArgs().IncrementReceiptCountsAsync(default, default, default, default);
    }

    [Fact]
    public async Task Viewed_receipt_can_be_confirmed_without_counting_another_view()
    {
        var id = Guid.NewGuid();
        var member = Guid.NewGuid();
        _announcementRepository.GetAsync(id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new SessionAnnouncement(id, Guid.NewGuid(), 1, Guid.NewGuid(), 2, "title", "body"));
        var receipt = new SessionAnnouncementReceipt(id, member, 2, DateTime.UtcNow);
        _receiptRepository.FindByUnitAsync(id, member, Arg.Any<CancellationToken>()).Returns(receipt);
        Assert.True((await _manager.ConfirmReceiptAsync(id, member, 2)).IsConfirmed);
        await _announcementRepository.Received(1).IncrementReceiptCountsAsync(id, 0, 1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Revoked_announcement_does_not_accept_receipts()
    {
        var id = Guid.NewGuid();
        _announcementRepository.GetAsync(id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new SessionAnnouncement(id, Guid.NewGuid(), 1, Guid.NewGuid(), 2, "title", "body", status: AnnouncementStatus.Revoked));
        await Assert.ThrowsAsync<UserFriendlyException>(() => _manager.RecordViewAsync(id, Guid.NewGuid(), 2));
        await _receiptRepository.DidNotReceiveWithAnyArgs().InsertAsync(default, default, default);
    }
}
