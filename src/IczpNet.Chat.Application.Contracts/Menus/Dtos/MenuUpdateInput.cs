using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Menus.Dtos;

public class MenuUpdateInput : BaseTreeInputDto<Guid>
{
    /// <summary>
    /// 说明
    /// </summary>
    public virtual string Description { get; set; }

    /// <summary>
    /// 事件类型 (Action/Click, Navigation/View 等)
    /// </summary>
    public virtual ActionEvents Event { get; set; }

    /// <summary>
    /// 菜单编码/Key
    /// </summary>
    [MaxLength(64)]
    public virtual string Code { get; set; }

    /// <summary>
    /// 参数/URL
    /// </summary>
    [MaxLength(500)]
    public virtual string Args { get; set; }
}
