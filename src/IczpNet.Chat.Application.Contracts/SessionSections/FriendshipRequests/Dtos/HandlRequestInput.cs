using System;
using System.Collections.Generic;
using System.Text;

namespace IczpNet.Chat.SessionSections.FriendshipRequests.Dtos
{
    public class HandlRequestInput
    {
        public virtual Guid FriendshipRequestId { get; set; }
        public virtual bool IsAgreed { get; set; }
        public virtual string HandlMessage { get; set; }
    }
}
