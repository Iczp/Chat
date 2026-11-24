using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class SetPinnedInput
{
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    public virtual bool IsPinned { get; set; }

    /// <summary>
    /// 置顶排序权重（0=不置顶，数值越大越置顶）
    /// </summary>
    public virtual double? Sorting { get; set; }
}
