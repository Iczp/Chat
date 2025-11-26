using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitCacheDto: SessionUnitCacheItem
{

    public virtual SessionUnitSettingCacheItem Settings { get; set; }

    public virtual ChatObjectInfo Destination { get; set; }
}
