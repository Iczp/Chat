using System;

namespace IczpNet.Chat.SessionRequests;

public enum SessionRequestNotificationKind
{
    Requested = 1,
    Accepted = 2,
    Declined = 3,
}

public class SessionRequestNotificationArgs
{
    public Guid RequestId { get; set; }

    public SessionRequestNotificationKind Kind { get; set; }

    // Message.ClientMessageId is limited to 26 characters. The full Guid is
    // encoded into 22 base64url characters, with a distinct prefix per kind.
    public static string GetClientMessageId(Guid requestId, SessionRequestNotificationKind kind)
    {
        if (requestId == Guid.Empty) throw new ArgumentException("RequestId is required.", nameof(requestId));
        if (kind is < SessionRequestNotificationKind.Requested or > SessionRequestNotificationKind.Declined)
            throw new ArgumentOutOfRangeException(nameof(kind), kind, null);

        var encoded = Convert.ToBase64String(requestId.ToByteArray())
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"SR{(int)kind}{encoded}";
    }
}
