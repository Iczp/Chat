using System;

namespace IczpNet.Chat.ConnectionPools;

public class ConnectionPool: IConnectionPool
{
    /// <summary>
    /// 
    /// </summary>
    public virtual string ConnectionId { get; set; }

    /// <summary>
    /// AppId
    /// </summary>
    public virtual string AppId { get; set; }

    /// <summary>
    ///  AppName
    /// </summary>
    public virtual string AppName { get; set; }

    /// <summary>
    /// hubs/ChatHub?id={ClientId}
    /// </summary>
    public virtual string QueryId { get; set; }

    /// <summary>
    /// Jwt ClientId
    /// </summary>
    public virtual string ClientId { get; set; }

    /// <summary>
    /// Jwt ClientName
    /// </summary>
    public virtual string ClientName { get; set; }

    /// <summary>
    /// Dns.GetHostName()
    /// </summary>
    public virtual string Host { get; set; }

    /// <summary>
    /// UserId
    /// </summary>
    public virtual Guid? UserId { get; set; }

    /// <summary>
    /// UserName
    /// </summary>
    public virtual string UserName { get; set; }

    /// <summary>
    /// DeviceId
    /// </summary>
    public virtual string DeviceId { get; set; }

    /// <summary>
    /// DeviceType
    /// </summary>
    public virtual string DeviceType { get; set; }

    /// <summary>
    /// IpAddress
    /// </summary>
    public virtual string IpAddress { get; set; }

    /// <summary>
    /// BrowserInfo
    /// </summary>
    public virtual string BrowserInfo { get; set; }

    /// <summary>
    /// DeviceInfo
    /// </summary>
    public virtual string DeviceInfo { get; set; }

    /// <summary>
    /// ActiveTime
    /// </summary>
    public virtual DateTime? ActiveTime { get; set; }

    /// <summary>
    /// CreationTime
    /// </summary>
    public virtual DateTime CreationTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual Object Extra { get; set; }
    

    public override string ToString()
    {
        return $"{nameof(AppId)}={AppId},{nameof(ClientId)}={ClientId},{nameof(ConnectionId)}={ConnectionId},{nameof(UserId)}={UserId},{nameof(UserName)}={UserName},{nameof(DeviceId)}={DeviceId}";
    }
}
