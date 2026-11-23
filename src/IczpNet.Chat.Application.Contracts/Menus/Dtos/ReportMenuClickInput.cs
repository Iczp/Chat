using System;

namespace IczpNet.Chat.Menus.Dtos;

/// <summary>
/// 菜单点击上报输入参数
/// </summary>
public class ReportMenuClickInput
{
    /// <summary>
    /// 触发人会话单元Id（可选，若有则提取对应用户/对象）
    /// </summary>
    public virtual Guid? SessionUnitId { get; set; }

    /// <summary>
    /// 触发人/操作者 ChatObjectId（可选）
    /// </summary>
    public virtual long? SenderId { get; set; }

    /// <summary>
    /// 客户端设备类型或标示（可选）
    /// </summary>
    public virtual string DeviceType { get; set; }
}

/// <summary>
/// 单个菜单点击统计项
/// </summary>
public class MenuItemStatisticDto
{
    public virtual Guid MenuId { get; set; }

    public virtual string Name { get; set; }

    public virtual string Event { get; set; }

    public virtual string Code { get; set; }

    /// <summary>
    /// 总点击次数 (PV)
    /// </summary>
    public virtual long TotalClicks { get; set; }

    /// <summary>
    /// 独立点击人数 (UV)
    /// </summary>
    public virtual long UniqueUsers { get; set; }

    /// <summary>
    /// 今日点击次数
    /// </summary>
    public virtual long TodayClicks { get; set; }

    /// <summary>
    /// 今日独立点击人数
    /// </summary>
    public virtual long TodayUsers { get; set; }
}

/// <summary>
/// 公众号菜单整体统计看板 DTO
/// </summary>
public class OfficialMenuStatisticOutput
{
    public virtual long ChatObjectId { get; set; }

    /// <summary>
    /// 该公众号下所有菜单累计总点击量
    /// </summary>
    public virtual long TotalClicks { get; set; }

    /// <summary>
    /// 今日总点击量
    /// </summary>
    public virtual long TodayClicks { get; set; }

    /// <summary>
    /// 各菜单项详细统计
    /// </summary>
    public virtual System.Collections.Generic.List<MenuItemStatisticDto> Items { get; set; } = [];
}
