using System;

namespace IczpNet.Chat.ConnectionPools.Dtos;

/// <summary>
/// 连接池信息
/// </summary>
[Serializable]
public class ConnectionPoolDto : ConnectionPoolCacheItem
{
    ///// <summary>
    ///// ChatObjectIdList
    ///// </summary>
    //public virtual List<long> ChatObjectIdList { get; set; } = [];

    //public override string ToString()
    //{
    //    return $"{nameof(ConnectionId)}={ConnectionId},{nameof(UserId)}={UserId},{nameof(UserName)}={UserName},{nameof(DeviceId)}={DeviceId},{nameof(ChatObjectIdList)}=[{ChatObjectIdList.JoinAsString(",")}]";
    //}
}
