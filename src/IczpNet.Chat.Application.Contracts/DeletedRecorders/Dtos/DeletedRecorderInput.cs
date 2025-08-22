using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.DeletedRecorders.Dtos;

public class DeletedRecorderInput
{
    //public virtual long SessionUnitId { get; set; }

    //public virtual long DestinationId { get; set; }

    /// <summary>
    /// 会话单元
    /// </summary>
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    /// <summary>
    /// 消息id
    /// </summary>
    [Required]
    public virtual long MessageId { get; set; }

    /// <summary>
    /// 设备id
    /// </summary>
    public virtual string DeviceId { get; set; }

}
