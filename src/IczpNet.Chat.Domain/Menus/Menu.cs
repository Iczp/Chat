using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Menus;

[Index(nameof(OwnerId))]
[Index(nameof(FullPath))]
[Index(nameof(Sorting), AllDescending = true)]
[Index(nameof(OwnerId), nameof(ParentId), nameof(Sorting), IsDescending = new[] { false, false, true })]
public class Menu : BaseTreeEntity<Menu, Guid>, IChatOwner<long>
{
    public virtual long OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; set; }

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
