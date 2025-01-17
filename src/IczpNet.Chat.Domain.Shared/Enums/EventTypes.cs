using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 事件类型
/// </summary>
[Description("事件类型")]
public enum EventTypes : int
{
    /// <summary>
    /// 新增
    /// </summary>
    [Description("新增")]
    Created = 0,
    /// <summary>
    /// 修改
    /// </summary>
    [Description("修改")]
    Updated = 1,
    /// <summary>
    /// 变更
    /// </summary>
    [Description("变更")]
    Changed = 2,
    /// <summary>
    /// 删除
    /// </summary>
    [Description("删除")]
    Deleted = 3,
    
}
