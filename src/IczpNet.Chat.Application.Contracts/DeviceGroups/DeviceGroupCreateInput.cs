using System;
using System.ComponentModel.DataAnnotations;
namespace IczpNet.Chat.DeviceGroups;

///<summary>
/// 新增 
///</summary>
[Serializable]
public class DeviceGroupCreateInput
{
    public string AppId { get; set; }
    public string ConcurrencyStamp { get; set; }


    ///<summary>
    /// 名称 
    ///</summary>
    public virtual string Name { get; set; }

    ///<summary>
    /// 说明 
    ///</summary>
    public virtual string Description { get; set; }
}