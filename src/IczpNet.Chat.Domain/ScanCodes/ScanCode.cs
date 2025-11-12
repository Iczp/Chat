using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.ScanCodes;

public class ScanCode : BaseEntity<Guid>, IDeviceId
{
    /// <summary>
    /// 条码类型 QR_CODE
    /// </summary>
    [Comment("条码类型")]
    [StringLength(36)]
    public virtual string Type { get; set; }

    /// <summary>
    /// 条码内容
    /// </summary>
    [Comment("条码内容")]
    [StringLength(1024)]
    public virtual string Content { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [StringLength(128)]
    [Comment("设备ID")]
    public virtual string DeviceId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Comment("用户Id")]
    public virtual Guid? UserId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [StringLength(128)]
    [Comment("ClientId")]
    public virtual string ClientId { get; set; }

    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    [Comment("执行时间（毫秒）")]
    public virtual double Execution { get; set; }

    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    [Comment("处理器个数")]
    public virtual int HandlerCount { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual IList<ScanHandler> ScanHandlerList { get; set; } = [];

    /// <summary>
    /// 
    /// </summary>
    [NotMapped]
    public virtual IList<ScanHandler> ScanHandlers => ScanHandlerList.Where(x => x.Success).ToList();

    public virtual bool IsQrCode()
    {
        return Type == "QR_CODE";
    }
}
