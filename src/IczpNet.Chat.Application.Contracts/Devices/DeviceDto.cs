using IczpNet.Chat.DeviceGroups;
using System;
using System.Collections.Generic;
using Volo.Abp.Auditing;

namespace IczpNet.Chat.Devices;

///<summary>
/// 设备 
///</summary>
[Serializable]
public class DeviceDto : DeviceSampleDto, IHasModificationTime
{
    /// <summary>
    /// 
    /// </summary>
    public virtual List<DeviceGroupSampleDto> Groups { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastModificationTime { get; set; }
}