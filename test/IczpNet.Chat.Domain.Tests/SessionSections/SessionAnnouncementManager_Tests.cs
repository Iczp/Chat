using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Announcements;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IczpNet.Chat.SessionSections;

public class SessionAnnouncementManager_Tests
{
    private readonly ISessionAnnouncementRepository _announcementRepository;
    private readonly ISessionAnnouncementReceiptRepository _receiptRepository;
    private readonly SessionAnnouncementManager _manager;

    public SessionAnnouncementManager_Tests()
    {
        _announcementRepository = Substitute.For<ISessionAnnouncementRepository>();
        _receiptRepository = Substitute.For<ISessionAnnouncementReceiptRepository>();
        _manager = new SessionAnnouncementManager(_announcementRepository, _receiptRepository);
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
        Assert.Equal(1, announcement.ReadCount);
        Assert.Equal(0, announcement.ConfirmedCount);

        await _receiptRepository.Received(1).InsertAsync(Arg.Any<SessionAnnouncementReceipt>(), true, Arg.Any<CancellationToken>());
        await _announcementRepository.Received(1).UpdateAsync(announcement, true, Arg.Any<CancellationToken>());
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
        Assert.Equal(1, announcement.ConfirmedCount);
        Assert.Equal(1, announcement.ReadCount);
    }
}
