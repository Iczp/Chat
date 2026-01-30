using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionUnitSettings;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitFriendDetailDto : SessionUnitCacheItem
{
    public virtual ChatObjectInfo Destination { get; set; }

    public virtual ChatObjectInfo Owner { get; set; }

    public virtual SessionUnitSettingCacheItem Setting { get; set; }
}
