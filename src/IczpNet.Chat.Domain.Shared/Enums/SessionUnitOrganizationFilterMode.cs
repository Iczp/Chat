namespace IczpNet.Chat.Enums;

/// <summary>
/// 群成员组织过滤模式
/// </summary>
public enum SessionUnitOrganizationFilterMode
{
    /// <summary>
    /// 全部成员
    /// </summary>
    All = 0,

    /// <summary>
    /// 未分组（不属于任何组织）
    /// </summary>
    Ungrouped = 1,

    /// <summary>
    /// 指定组织
    /// </summary>
    Organization = 2
}
