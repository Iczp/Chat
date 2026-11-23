using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Menus;

/// <summary>
/// 已发布的正式菜单快照缓存模型
/// </summary>
[Serializable]
public class PublishedMenuCacheItem
{
    /// <summary>
    /// 公众号/订阅号 ChatObjectId
    /// </summary>
    public virtual long ChatObjectId { get; set; }

    /// <summary>
    /// 当前发布版本号（Unix 时间戳毫秒数）
    /// </summary>
    public virtual string Version { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    public virtual DateTime PublishTime { get; set; }

    /// <summary>
    /// 已发布的菜单节点列表（包含层级关系及 Children 树）
    /// </summary>
    public virtual List<PublishedMenuItem> Items { get; set; } = [];
}

/// <summary>
/// 已发布的单个菜单项
/// </summary>
[Serializable]
public class PublishedMenuItem
{
    public virtual Guid Id { get; set; }

    public virtual Guid? ParentId { get; set; }

    public virtual string Name { get; set; }

    public virtual long OwnerId { get; set; }

    public virtual string Event { get; set; }

    public virtual string Code { get; set; }

    public virtual string Args { get; set; }

    public virtual double Sorting { get; set; }

    public virtual List<PublishedMenuItem> Children { get; set; } = [];
}
