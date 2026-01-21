using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionBoxes;

/// <summary>
/// 会话盒子
/// </summary>
[Index(nameof(Name))]
[Comment("会话盒子")]
public class Box : BaseEntity<Guid>
{

    /// <summary>
    /// 名称
    /// </summary>
    [Comment("名称")]
    [StringLength(64)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    [Comment("说明")]
    [StringLength(512)]
    public virtual string Description { get; set; }

    /// <summary>
    /// 所属性人
    /// </summary>
    public virtual long? OwnerId { get; set; }

    /// <summary>
    /// 所属性人
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [InverseProperty(nameof(SessionUnit.Box))]
    public virtual IList<SessionUnit> SessionUnitList { get; set; } = [];
}
