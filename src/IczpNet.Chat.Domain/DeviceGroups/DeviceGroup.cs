using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.DeviceGroupMaps;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

    /// <summary>
    /// 设备数量
    /// </summary>
    [NotMapped]
    public virtual int DeviceCount => DeviceGroupMapList.Select(x => x.Device.IsEnabled && !x.Device.IsDeleted).Count();

    public virtual int SetDevices(List<Guid> deviceIdList)
    {
        DeviceGroupMapList.Clear();
        DeviceGroupMapList = deviceIdList.Select(x => new DeviceGroupMap()
        {
            DeviceGroupId = Id,
            DeviceId = x,
        }).ToList();

        return DeviceGroupMapList.Count;

    }
}
