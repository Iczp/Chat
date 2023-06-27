using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitGetCounterInput
{
    /// <summary>
    /// 会话单元Id
    /// </summary>
    [Required]
    public Guid SessionUnitId { get; set; }

    /// <summary>
    /// 最小消息Id
    /// </summary>
    [Required]
    public long MinMessageId { get; set; }

    /// <summary>
    /// 是否包含免打扰
    /// </summary>
    [DefaultValue(false)]
    public bool? IsImmersed { get; set; } = false;
}
