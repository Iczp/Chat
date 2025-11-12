using IczpNet.Chat.BaseDtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.ScanCodes;

public class ScanCodeDto : BaseDto<Guid>
{
    /// <summary>
    /// 条码类型
    /// </summary>

    public virtual string Type { get; set; }

    /// <summary>
    /// 条码内容
    /// </summary>

    public virtual string Content { get; set; }

    /// <summary>
    /// 
    /// </summary>

    public virtual string DeviceId { get; set; }

    /// <summary>
    /// 
    /// </summary>

    public virtual Guid? UserId { get; set; }

    /// <summary>
    /// 
    /// </summary>

    public virtual string ClientId { get; set; }

    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    public virtual double Execution { get; set; }

    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    public virtual int HandlerCount { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual IList<ScanHandlerDto> ScanHandlerList { get; set; }


}
