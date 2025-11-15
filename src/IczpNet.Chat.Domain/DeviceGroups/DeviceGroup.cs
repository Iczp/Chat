using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.DeviceGroupMaps;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.DeviceGroups;

public class DeviceGroup : BaseEntity<Guid>
{
    /// <summary>
    /// 
    /// </summary>
    [StringLength(64)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual IList<DeviceGroupMap> DeviceGroupMapList { get; set; } = [];
}
