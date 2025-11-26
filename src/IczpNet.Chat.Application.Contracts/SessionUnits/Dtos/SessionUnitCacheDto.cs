using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using System;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitCacheDto
{
    public Guid Id { get; set; }

    public virtual SessionUnitCacheItem Item { get; set; }

    public virtual SessionUnitSettingCacheItem Settings { get; set; }

    public virtual ChatObjectInfo Destination { get; set; }
}
