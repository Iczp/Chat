using System;
namespace IczpNet.Chat.DeviceGroups;

///<summary>
/// Dto 
///</summary>
[Serializable]
public class DeviceGroupDto : DeviceGroupSampleDto
{

    ///<summary>
    /// 说明 
    ///</summary>
    public virtual string Description { get; set; }

    /// <summary>
    /// 设备数量
    /// </summary>
    public virtual int DeviceCount { get; set; }

}