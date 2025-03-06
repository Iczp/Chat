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
    public string ConnectionId { get; set; }

    /// <summary>
    /// hubs/ChatHub?id={QueryId}
    /// </summary>
    public string QueryId { get; set; }

    /// <summary>
    /// Dns.GetHostName()
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// AppUserId
    /// </summary>
    public Guid AppUserId { get; set; }

    /// <summary>
    /// DeviceId
    /// </summary>
    public string DeviceId { get; set; }

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
    public DateTime? CreationTime { get; set; } = DateTime.Now;

    /// <summary>
    /// ChatObjectIdList
    /// </summary>
    public List<long> ChatObjectIdList { get; set; } = [];

    public override string ToString()
    {
        return $"{nameof(ConnectionId)}={ConnectionId},{nameof(AppUserId)}={AppUserId},{nameof(DeviceId)}={DeviceId},{nameof(ChatObjectIdList)}=[{ChatObjectIdList.JoinAsString(",")}]";
    }
}
