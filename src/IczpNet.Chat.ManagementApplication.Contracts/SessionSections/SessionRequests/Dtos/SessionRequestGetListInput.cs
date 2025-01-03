﻿using IczpNet.Chat.Management.BaseDtos;
using System;

namespace IczpNet.Chat.Management.SessionSections.SessionRequests.Dtos;

public class SessionRequestGetListInput : BaseGetListInput
{
    public virtual long? OwnerId { get; set; }

    public virtual long? DestinationId { get; set; }

    public virtual bool? IsEnabled { get; set; }

    public virtual bool? IsExpired { get; set; }

    public virtual bool? IsHandled { get; set; }

    public virtual bool? IsAgreed { get; set; }

    public virtual DateTime? StartHandleTime { get; set; }

    public virtual DateTime? EndHandleTime { get; set; }

    public virtual DateTime? StartCreationTime { get; set; }

    public virtual DateTime? EndCreationTime { get; set; }
}
