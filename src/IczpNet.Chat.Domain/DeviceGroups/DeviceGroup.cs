using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.DeviceGroupMaps;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.DeviceGroups;

/// <summary>
/// 
/// </summary>
[Index(nameof(Name), AllDescending = false)]
[Comment("设备分组")]
public class DeviceGroup : BaseEntity<Guid>
{
    /// <summary>
    /// 
    /// </summary>
    [StringLength(64)]
    public virtual string Name { get; set; }

    ///<summary>
    /// 说明 
    ///</summary>
    [StringLength(500)]
    public virtual string Description { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual IList<DeviceGroupMap> DeviceGroupMapList { get; protected set; } = [];
}
