using IczpNet.Chat.ChatObjects;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitFriendDetailDto : SessionUnitFriendDto
{
    public virtual long SessionUnitCount { get; set; }

    public virtual ChatObjectInfo Owner { get; set; }
}
