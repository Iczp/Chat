namespace IczpNet.Chat.Enums;

/// <summary>
/// 群所有权治理状态
/// </summary>
public enum RoomOwnershipState
{
    /// <summary>
    /// 正常（存在唯一有效真实群主）
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// 无有效群主，存在有效管理员（管理员可接任）
    /// </summary>
    OwnerlessWithAdmins = 1,

    /// <summary>
    /// 无有效群主且无有效管理员（有效成员可认领）
    /// </summary>
    OwnerlessWithoutAdmins = 2,

    /// <summary>
    /// 无可接任的有效真实成员
    /// </summary>
    Empty = 3,

    /// <summary>
    /// 治理状态异常（例如存在多个有效群主）
    /// </summary>
    Inconsistent = 4,

    /// <summary>
    /// 群已解散或停用
    /// </summary>
    Dissolved = 5
}

/// <summary>
/// 当前成员可执行的所有权恢复动作
/// </summary>
public enum RoomOwnershipRecoveryAction
{
    None = 0,
    AdminTakeover = 1,
    MemberClaim = 2,
    ManualRecovery = 3
}
