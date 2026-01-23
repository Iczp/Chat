using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits;

public class MemberMaps
{
    public Dictionary<SessionUnitElement, long> Pinned { get; set; } = [];
    public Dictionary<SessionUnitElement, bool> Immersed { get; set; } = [];
    public Dictionary<SessionUnitElement, bool> Creator { get; set; } = [];
    public Dictionary<SessionUnitElement, bool> Private { get; set; } = [];
    public Dictionary<SessionUnitElement, bool> Static { get; set; } = [];
    public Dictionary<SessionUnitElement, Guid> Box { get; set; } = [];
}
