using System;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话成员变更分布式事件传输对象
/// </summary>
[Serializable]
public class SessionUnitMemberUpdatedDistributedEto
{
    public Guid SessionId { get; set; }

    public Guid SessionUnitId { get; set; }

    public long MemberChatObjectId { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsCreator { get; set; }

    public double Score { get; set; }

    public string MemberName { get; set; }

    public DateTime CreationTime { get; set; }
}
