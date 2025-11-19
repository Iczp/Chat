using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Devices;

/// <summary>
/// SampleDto
/// </summary>
[Serializable]
public class DeviceSampleDto : BaseDto<Guid>
{
    /// <summary>
    /// 显示名称
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// 设备 ID
    /// </summary>
    public virtual string DeviceId { get; set; }

    /// <summary>
    /// 客户端平台
    /// </summary>
    public virtual string Platform { get; set; }

    /// <summary>
    /// manifest.json 中应用appid
    /// </summary>
    public virtual string AppId { get; set; }

    /// <summary>
    /// 手机品牌（H5 不支持，可选）
    /// </summary>
    public virtual string Brand { get; set; }

    /// <summary>
    /// 设备品牌（H5 不支持，可选）
    /// </summary>
    public virtual string DeviceBrand { get; set; }

    /// <summary>
    /// 设备型号
    /// </summary>
    public virtual string DeviceModel { get; set; }

    /// <summary>
    /// 设备类型（phone/pad/pc/web/html5plus）
    /// </summary>
    public virtual string DeviceType { get; set; }

    /// <summary>
    /// 手机型号
    /// </summary>
    public virtual string Model { get; set; }

    /// <summary>
    /// 操作系统信息
    /// </summary>
    public virtual string System { get; set; }

    /// <summary>
    /// 是否可用
    /// </summary>
    public virtual bool IsEnabled { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string Remarks { get; set; }

}