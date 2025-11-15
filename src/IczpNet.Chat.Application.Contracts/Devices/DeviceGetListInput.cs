using IczpNet.Chat.BaseDtos;
using System;
using System.ComponentModel.DataAnnotations;
namespace IczpNet.Chat.Devices;

///<summary>
/// 查询列表 
///</summary>
[Serializable]
public class DeviceGetListInput : GetListInput
{
    /// <summary>
    /// 设备 ID
    /// </summary>
    [MaxLength(128)]
    public virtual string DeviceId { get; set; }

    /// <summary>
    /// 客户端平台
    /// </summary>
    [MaxLength(64)]
    public virtual string Platform { get; set; }

    /// <summary>
    /// manifest.json 中应用appid
    /// </summary>
    [MaxLength(64)]
    public virtual string AppId { get; set; }
}