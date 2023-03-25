using System;

namespace IczpNet.Chat.SessionSections.FriendshipRequests.Dtos
{
    public class HandleRequestInput
    {
        public virtual Guid FriendshipRequestId { get; set; }
        public virtual bool IsAgreed { get; set; }
        public virtual string HandleMessage { get; set; }
    }
}
