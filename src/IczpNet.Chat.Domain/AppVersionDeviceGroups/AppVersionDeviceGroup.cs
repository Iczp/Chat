using IczpNet.Chat.AppVersions;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.DeviceGroups;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.AppVersionDeviceGroups;

public class AppVersionDeviceGroup : BaseEntity
{
    /// <summary>
    /// 
    /// </summary>
    public virtual Guid AppVersionId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(AppVersionId))]
    public virtual AppVersion AppVersion { get; set; }

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
        return [DeviceGroupId, AppVersionId];
    }
}
