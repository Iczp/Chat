using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionUnitSettings;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitFriendDetailDto : SessionUnitCacheItem
{

    public virtual long SessionUnitCount { get; set; }

    public virtual ChatObjectInfo Destination { get; set; }

    public virtual ChatObjectInfo Owner { get; set; }

    public virtual SessionUnitSettingCacheItem Setting { get; set; }

    /// <summary>
    /// friend Score
    /// </summary>
    public virtual double? Score { get; set; }
}
