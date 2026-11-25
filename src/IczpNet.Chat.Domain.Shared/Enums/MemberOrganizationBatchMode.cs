namespace IczpNet.Chat.Enums;

/// <summary>
/// 批量设置成员组织的操作模式
/// </summary>
public enum MemberOrganizationBatchMode
{
    /// <summary>
    /// 追加到组织（保留已有组织，支持多组织归属）
    /// </summary>
    Add = 0,

    /// <summary>
    /// 从所选组织移除
    /// </summary>
    Remove = 1,

    /// <summary>
    /// 重新设置组织（覆盖已有组织归属，目标组织不可为空）
    /// </summary>
    Replace = 2,

    /// <summary>
    /// 清空全部组织（设为未分组，目标组织必须为空）
    /// </summary>
    Clear = 3
}
