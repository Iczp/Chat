using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Connections.Dtos;

public class ConnectionGetListInput : GetListInput
{
    /// <summary>
    /// 是否在线
    /// </summary>
    public virtual bool? IsOnline { get; set; }

    /// <summary>
    /// IM服务器Id
    /// </summary>
    public virtual string ServerHostId { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public virtual Guid? AppUserId { get; set; }

    /// <summary>
    /// 设备Id
    /// </summary>
    public virtual string DeviceId { get; set; }

    /// <summary>
    /// Ip地址
    /// </summary>
    public virtual string IpAddress { get; set; }

    /// <summary>
    /// 起始创建时间
    /// </summary>
    public virtual DateTime? StartCreationTime { get; set; }

    /// <summary>
    /// 终始创建时间
    /// </summary>
    public virtual DateTime? EndCreationTime { get; set; }
}
