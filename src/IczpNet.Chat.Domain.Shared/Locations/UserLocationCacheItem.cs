using System;

namespace IczpNet.Chat.Locations;

/// <summary>
/// 客户端位置信息
/// </summary>
public class UserLocationCacheItem
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual Guid? UserId { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// 手机品牌
    /// </summary>
    public virtual string Brand { get; set; }

    /// <summary>
    /// 手机型号
    /// </summary>
    public virtual string Model { get; set; }

    /// <summary>
    /// 设备Id（唯一识别码）
    /// </summary>
    public virtual string DeviceId { get; set; }

    /// <summary>
    /// 坐标 Latitude
    /// </summary>
    public virtual float Latitude { get; set; }

    /// <summary>
    /// 坐标 Longitude
    /// </summary>
    public virtual float Longitude { get; set; }

    /// <summary>
    /// 最近活动时间
    /// </summary>
    public virtual DateTime ActiveTime { get; set; } = DateTime.Now;
}
