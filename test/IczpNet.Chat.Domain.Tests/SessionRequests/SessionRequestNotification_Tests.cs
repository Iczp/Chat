using System;
using Shouldly;
using Xunit;

namespace IczpNet.Chat.SessionRequests;

public class SessionRequestNotification_Tests
{
    [Fact]
    public void ClientMessageId_is_stable_bounded_and_distinct_by_kind()
    {
        var requestId = Guid.NewGuid();
        var requested = SessionRequestNotificationArgs.GetClientMessageId(requestId, SessionRequestNotificationKind.Requested);
        var accepted = SessionRequestNotificationArgs.GetClientMessageId(requestId, SessionRequestNotificationKind.Accepted);
        var declined = SessionRequestNotificationArgs.GetClientMessageId(requestId, SessionRequestNotificationKind.Declined);

        requested.Length.ShouldBeLessThanOrEqualTo(26);
        requested.ShouldBe(SessionRequestNotificationArgs.GetClientMessageId(requestId, SessionRequestNotificationKind.Requested));
        requested.ShouldNotBe(accepted);
        requested.ShouldNotBe(declined);
        accepted.ShouldNotBe(declined);
        SessionRequestNotificationArgs.GetClientMessageId(Guid.NewGuid(), SessionRequestNotificationKind.Requested)
            .ShouldNotBe(requested);
    }

    [Fact]
    public void Invalid_notification_identity_is_rejected()
    {
        Should.Throw<ArgumentException>(() =>
            SessionRequestNotificationArgs.GetClientMessageId(Guid.Empty, SessionRequestNotificationKind.Requested));
        Should.Throw<ArgumentOutOfRangeException>(() =>
            SessionRequestNotificationArgs.GetClientMessageId(Guid.NewGuid(), (SessionRequestNotificationKind)99));
    }
}
