using IczpNet.Chat.BaseDtos;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitChangesGetListInput : GetListInput
{
    /// <summary>
    /// 所属人 ChatObjectId
    /// </summary>
    [Required]
    public virtual long OwnerId { get; set; }

    /// <summary>
    /// 朋友 ChatObjectId
    /// </summary>
    public virtual long? DestinationId { get; set; }

    /// <summary>
    /// 会话单元 SessionUnitId
    /// </summary>
    public virtual Guid? SessionUnitId { get; set; }

    /// <summary>
    /// 会话 SessionId
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// 最小Ticks
    /// </summary>
    public virtual double? MinTicks { get; set; }

    /// <summary>
    /// 最小Ticks
    /// </summary>
    public virtual double? MaxTicks { get; set; }
}
