using IczpNet.Chat.ChatObjects;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitMemberDetailDto : SessionUnitMemberDto
{
    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectInfo Destination { get; set; }
}
