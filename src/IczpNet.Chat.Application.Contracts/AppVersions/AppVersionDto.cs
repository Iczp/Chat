using IczpNet.Chat.DeviceGroups;
using System;
using System.Collections.Generic;
namespace IczpNet.Chat.AppVersions;

///<summary>
/// Dto 
///</summary>
[Serializable]
public class AppVersionDto : AppVersionSampleDto
{

    /////<summary>
    ///// 说明 
    /////</summary>
    //public virtual string Description { get; set; } 

    /// <summary>
    /// 
    /// </summary>
    public virtual List<DeviceGroupSampleDto> Groups { get; set; }

}