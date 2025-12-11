using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitSearchCacheItem(List<Guid> unitIds)
{
    public List<Guid> UnitIds { get; set; } = unitIds;

}
