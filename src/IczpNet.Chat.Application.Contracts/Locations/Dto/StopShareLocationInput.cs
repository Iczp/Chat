using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Locations;


public class StopShareLocationInput
{
    /// <summary>
    /// 会话单元
    /// </summary>
    [Required]
    public Guid SessionUnitId { get; set; }
}
