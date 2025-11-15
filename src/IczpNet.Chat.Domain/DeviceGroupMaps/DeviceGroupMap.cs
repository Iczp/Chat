using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.DeviceGroups;
using IczpNet.Chat.Devices;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.DeviceGroupMaps;

public class DeviceGroupMap : BaseEntity
{
    /// <summary>
    /// 
    /// </summary>
    public virtual Guid DeviceId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(DeviceId))]
    public virtual Device Device { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual Guid DeviceGroupId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(DeviceGroupId))]
    public virtual DeviceGroup DeviceGroup { get; set; }

    public override object[] GetKeys()
    {
        return [DeviceGroupId, DeviceId];
    }
}
