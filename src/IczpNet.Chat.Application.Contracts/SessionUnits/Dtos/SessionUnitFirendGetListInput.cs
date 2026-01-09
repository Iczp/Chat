using IczpNet.Chat.Enums;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitFirendGetListInput : SessionUnitLatestGetListInput
{
    /// <summary>
    /// 好友归档
    /// 0: All 全部,
    /// 1: Pinned 置顶,
    /// 2: Following 关注,
    /// 3: RemindAll @所有人,
    /// 4: RemindMe @我,
    /// 5: Immersed 静默,
    /// 6: Creator 创建人,
    /// </summary>
    public FriendTypes FriendType { get; set; }
}
