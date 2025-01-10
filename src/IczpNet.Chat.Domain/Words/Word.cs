using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Words;

/// <summary>
/// 关键字
/// </summary>
[Index(nameof(Value), AllDescending = true)]
public class Word : BaseEntity<Guid>, IIsEnabled
{
    /// <summary>
    /// 
    /// </summary>
    [MaxLength(36)]
    public virtual string Value { get; set; }

    /// <summary>
    /// 是否可用
    /// </summary>
    public virtual bool IsEnabled { get; set; }

    /// <summary>
    /// 是否脏词
    /// </summary>
    public virtual bool IsDirty { get; set; }

    protected Word() { }
}
