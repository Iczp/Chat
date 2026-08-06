using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits;

[Serializable]
public class FlushSessionUnitJobArgs
{
    public int Count { get;  set; }
    public List<Guid> SessionUnitIds { get; set; } = [];
}