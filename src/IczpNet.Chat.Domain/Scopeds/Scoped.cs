﻿using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using System;

namespace IczpNet.Chat.Scopeds;

public class Scoped : BaseEntity
{
    protected Scoped() { }

    public virtual Guid SessionUnitId { get; set; }

    public virtual SessionUnit SessionUnit { get; set; }

    public virtual long MessageId { get; set; }

    public virtual Message Message { get; set; }

    public Scoped(Guid sessionUnitId)
    {
        SessionUnitId = sessionUnitId;
    }

    public override object[] GetKeys()
    {
        return [SessionUnitId, MessageId];
    }
}
