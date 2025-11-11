using System;

namespace IczpNet.Chat.ConnectionPools;

public interface IConnectionPool
{
    /// <summary>
    /// 
    /// </summary>
    string ConnectionId { get; set; }

    /// <summary>
    ///  PushClientId
    /// </summary>
    string PushClientId { get; set; }

    /// <summary>
    /// hubs/ChatHub?id={ClientId}
    /// </summary>
    string QueryId { get; set; }

    /// <summary>
    /// Jwt ClientId
    /// </summary>
    string ClientId { get; set; }

    /// <summary>
    /// Dns.GetHostName()
    /// </summary>
    string Host { get; set; }

    /// <summary>
    /// UserId
    /// </summary>
    Guid? UserId { get; set; }

    /// <summary>
    /// UserName
    /// </summary>
    string UserName { get; set; }

    /// <summary>
    /// DeviceId
    /// </summary>
    string DeviceId { get; set; }

    /// <summary>
    /// DeviceType
    /// </summary>
    string DeviceType { get; set; }

    /// <summary>
    /// IpAddress
    /// </summary>
    string IpAddress { get; set; }

    /// <summary>
    /// BrowserInfo
    /// </summary>
    string BrowserInfo { get; set; }

    /// <summary>
    /// DeviceInfo
    /// </summary>
    string DeviceInfo { get; set; }

    /// <summary>
    /// ActiveTime
    /// </summary>
    DateTime? ActiveTime { get; set; }

    /// <summary>
    /// CreationTime
    /// </summary>
    DateTime CreationTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    Object Extra { get; set; }
}
