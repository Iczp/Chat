using System;
using System.Collections.Generic;

namespace IczpNet.Chat.ConnectionPools;

/// <summary>
/// 连接池信息
/// </summary>
[Serializable]
public class ConnectionPoolCacheItem
{
    /// <summary>
    /// 
    /// </summary>
    public virtual string ConnectionId { get; set; }

    /// <summary>
    /// hubs/ChatHub?id={ClientId}
    /// </summary>
    public virtual string ClientId { get; set; }

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
    /// CreationTime
    /// </summary>
    public virtual DateTime? CreationTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ChatObjectIdList
    /// </summary>
    public virtual List<long> ChatObjectIdList { get; set; } = [];

    public override string ToString()
    {
        return $"{nameof(ConnectionId)}={ConnectionId},{nameof(UserId)}={UserId},{nameof(UserName)}={UserName},{nameof(DeviceId)}={DeviceId},{nameof(ChatObjectIdList)}=[{ChatObjectIdList.JoinAsString(",")}]";
    }
}
