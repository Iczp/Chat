using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.FavoritedRecorders.Dtos;

public class FavoritedRecorderCreateInput
{
    /// <summary>
    /// 会话单元Id
    /// </summary>
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    /// <summary>
    /// 消息Id
    /// </summary>
    [Required]
    public virtual long MessageId { get; set; }

    /// <summary>
    /// 设备Id
    /// </summary>
    public virtual string DeviceId { get; set; }
}
