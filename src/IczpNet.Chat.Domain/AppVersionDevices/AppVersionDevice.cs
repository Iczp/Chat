using IczpNet.Chat.AppVersions;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.Devices;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.AppVersionDevices;

public class AppVersionDevice : BaseEntity
{
    /// <summary>
    /// 
    /// </summary>
    public virtual Guid DeviceId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(DeviceId))]
    public virtual required Device Device { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual Guid AppVersionId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(DeviceId))]
    public virtual required AppVersion AppVersion { get; set; }

    public override object[] GetKeys()
    {
        return [DeviceId, AppVersionId];
    }
}
