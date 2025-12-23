using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.MessageWords;
using IczpNet.Chat.TextContentWords;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    [InverseProperty(nameof(MessageWord.Word))]
    public virtual IList<MessageWord> MessageWordList { get; set; } = [];


    [InverseProperty(nameof(TextContentWord.Word))]
    public virtual IList<TextContentWord> TextContentWordList { get; set; } = [];
    

    protected Word() { }
}
