using IczpNet.Chat.BaseDtos;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitLatestGetListInput : GetListInput
{
    /// <summary>
    /// OwnerId
    /// </summary>
    [Required]
    public long OwnerId { get; set; }

    /// <summary>
    /// 朋友Id
    /// </summary>
    public long? FriendId { get; set; }

    /// <summary>
    /// UnitId
    /// </summary>
    public Guid? UnitId { get; set; }

    /// <summary>
    /// SessionId
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// 最小Ticks
    /// </summary>
    public double? MinScore { get; set; }

    /// <summary>
    /// 最小Ticks
    /// </summary>
    public double? MaxScore { get; set; }
}
