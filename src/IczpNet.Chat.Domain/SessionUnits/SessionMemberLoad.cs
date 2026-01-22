using System;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 成员加载选项
/// </summary>
[Flags]
public enum SessionMemberLoad
{
    /// <summary>
    /// 不加载
    /// </summary>
    None = 0,
    /// <summary>
    /// 置顶
    /// </summary>
    Pinned = 1 << 0,
    /// <summary>
    /// 静默
    /// </summary>
    Immersed = 1 << 1,
    /// <summary>
    /// 创建人(群主)
    /// </summary>
    Creator = 1 << 2,
    /// <summary>
    /// 非公开(私有)
    /// </summary>
    Private = 1 << 3,
    /// <summary>
    /// 固定
    /// </summary>
    Static = 1 << 4,
    /// <summary>
    /// 消息盒子
    /// </summary>
    Box = 1 << 5,
}