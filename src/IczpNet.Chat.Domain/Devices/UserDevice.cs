using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Devices;

/// <summary>
/// 用户设备
/// </summary>
[Comment("用户设备")]
[Index(nameof(UserId))]
[Index(nameof(DeviceId))]
[Index(nameof(RawDeviceType))]
public class UserDevice : BaseEntity
{
    /// <summary>
    /// Abp UserId
    /// </summary>
    [Comment("Abp User Id")]
    public virtual Guid UserId { get; set; }

    /// <summary>
    /// Raw DeviceId
    /// </summary>
    [Comment("Raw DeviceId")]
    [StringLength(ChatConsts.DriveIdLength)]
    public virtual string RawDeviceId { get; set; }

    /// <summary>
    /// Raw DeviceType
    /// </summary>
    [Comment("Raw DeviceType")]
    [StringLength(ChatConsts.DriveTypeLength)]
    public virtual string RawDeviceType { get; set; }

    /// <summary>
    /// DeviceId
    /// </summary>
    [Comment(" Device Id")]
    public virtual Guid DeviceId { get; set; }

    /// <summary>
    /// Device
    /// </summary>
    [ForeignKey(nameof(DeviceId))]
    public virtual Device Device { get; set; }

    public UserDevice()
    {

    }

    public UserDevice(Guid userId, Device device)
    {
        UserId = userId;
        DeviceId = Device.Id;
        RawDeviceId = device?.DeviceId;
        RawDeviceType = device?.DeviceType;
    }

    public UserDevice(Guid userId, Guid deviceId, string rawDeviceId, string rawDeviceType)
    {
        UserId = userId;
        DeviceId = deviceId;
        RawDeviceId = rawDeviceId;
        RawDeviceType = rawDeviceType;
    }

    public override object[] GetKeys()
    {
        return [UserId, DeviceId];
    }
}



