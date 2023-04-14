using System;

namespace IczpNet.Chat.SessionSections.SessionRequests.Dtos
{
    public class SessionRequestHandleInput
    {
        public virtual Guid SessionRequestId { get; set; }
        public virtual bool IsAgreed { get; set; }
        public virtual string HandleMessage { get; set; }
    }
}
