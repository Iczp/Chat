using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits;

public class SessionMemberMaps
{
    public Dictionary<SessionUnitElement, long> Pinned { get; set; } = new();
    public Dictionary<SessionUnitElement, bool> Immersed { get; set; } = new();
    public Dictionary<SessionUnitElement, bool> Creator { get; set; } = new();
    public Dictionary<SessionUnitElement, bool> Private { get; set; } = new();
    public Dictionary<SessionUnitElement, bool> Static { get; set; } = new();
    public Dictionary<SessionUnitElement, Guid> Box { get; set; } = new();
}
