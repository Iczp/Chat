using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Menus.Dtos;

/// <summary>
/// 获取已发布菜单输出 DTO
/// </summary>
public class PublishedMenuOutput
{
    /// <summary>
    /// 公众号/订阅号 ChatObjectId
    /// </summary>
    public virtual long ChatObjectId { get; set; }

    /// <summary>
    /// 当前最新发布版本号
    /// </summary>
    public virtual string Version { get; set; }

    /// <summary>
    /// 是否有更新。若请求传来的 version 与线上一致，则为 false
    /// </summary>
    public virtual bool IsModified { get; set; }

    /// <summary>
    /// 已发布时间
    /// </summary>
    public virtual DateTime? PublishTime { get; set; }

    /// <summary>
    /// 已发布的菜单列表（仅在 IsModified 为 true 时填充，支持直接返回构建好的树或平铺列表）
    /// </summary>
    public virtual List<PublishedMenuItem> Items { get; set; } = [];
}
