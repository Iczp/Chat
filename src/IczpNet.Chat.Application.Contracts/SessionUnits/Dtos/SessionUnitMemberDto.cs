using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.SessionUnits;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitMemberDto : SessionUnitBase//SessionUnitCacheItem
{
    public virtual ChatObjectInfo Owner { get; set; }

    //public virtual SessionUnitSettingCacheItem Setting { get; set; }
}
