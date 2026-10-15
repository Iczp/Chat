using System;
namespace IczpNet.Chat.Devices;

///<summary>
/// 修改 
///</summary>
[Serializable]
public class DeviceUpdateInput : DeviceCreateInput
{
    [System.ComponentModel.DataAnnotations.MaxLength(64)]
    public string ConcurrencyStamp { get; set; }


}