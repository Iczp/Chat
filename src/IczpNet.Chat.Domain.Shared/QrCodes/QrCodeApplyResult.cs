using System;

namespace IczpNet.Chat.QrCodes;

[Serializable]
public class QrCodeApplyResult
{
    /// <summary>
    /// joined / added / request_sent / already_member / already_friend
    /// </summary>
    public virtual string Status { get; set; }

    public virtual string Message { get; set; }

    public virtual Guid? SessionUnitId { get; set; }

    public virtual Guid? SessionId { get; set; }

    public virtual Guid? SessionRequestId { get; set; }

    public virtual long? TargetChatObjectId { get; set; }
}
