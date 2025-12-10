using IczpNet.Chat.BaseDtos;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitCacheScoreGetListInput : GetListInput
{
    /// <summary>
    /// ChatObjectId
    /// </summary>
    [Required]
    public long OwnerId { get; set; }

    /// <summary>
    /// 最小Ticks
    /// </summary>
    public double? MinScore { get; set; }

    /// <summary>
    /// 最小Ticks
    /// </summary>
    public double? MaxScore { get; set; }
}
