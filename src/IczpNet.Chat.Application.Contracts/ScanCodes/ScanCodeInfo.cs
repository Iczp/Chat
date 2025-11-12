using System;
using System.Collections.Generic;

namespace IczpNet.Chat.ScanCodes;

public class ScanCodeInfo
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
    /// 
    /// </summary>
    public virtual IList<ScanHandlerInfo> ScanHandlerList { get; set; }
}
