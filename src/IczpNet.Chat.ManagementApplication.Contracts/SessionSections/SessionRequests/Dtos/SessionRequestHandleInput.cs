﻿using System;

namespace IczpNet.Chat.Management.SessionSections.SessionRequests.Dtos
{
    public class SessionRequestHandleInput
    {
        public virtual Guid SessionRequestId { get; set; }
        public virtual bool IsAgreed { get; set; }
        public virtual string HandleMessage { get; set; }
    }
}
