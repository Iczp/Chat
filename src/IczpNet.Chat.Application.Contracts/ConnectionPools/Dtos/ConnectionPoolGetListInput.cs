using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.ConnectionPools.Dtos;

public class ConnectionPoolGetListInput : GetListInput
{
    /// <summary>
    /// ConnectionId
    /// </summary>
    public string ConnectionId { get; set; }


    /// <summary>
    /// hubs/ChatHub?id={QueryId}
    /// </summary>
    public string QueryId { get; set; }

    /// <summary>
    /// Host
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public Guid? AppUserId { get; set; }

    /// <summary>
    /// 聊天对象Id
    /// </summary>
    public long? ChatObjectId { get; set; }
}
