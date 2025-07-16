using IczpNet.Chat.BaseDtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.ConnectionPools.Dtos;

public class ConnectionPoolGetListInput : GetListInput
{
    /// <summary>
    /// ConnectionId
    /// </summary>
    public string ConnectionId { get; set; }


    /// <summary>
    /// hubs/ChatHub?id={ClientId}
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Host
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 聊天对象Id
    /// </summary>
    public long? ChatObjectId { get; set; }

    /// <summary>
    /// 聊天对象Id
    /// </summary>
    public List<long> ChatObjectIdList { get; set; }

    /// <summary>
    /// 起始活跃时间
    /// </summary>
    public virtual DateTime? StartActiveTime { get; set; }

    /// <summary>
    /// 终始活跃时间
    /// </summary>
    public virtual DateTime? EndActiveTime { get; set; }

    /// <summary>
    /// 起始创建时间
    /// </summary>
    public virtual DateTime? StartCreationTime { get; set; }

    /// <summary>
    /// 终始创建时间
    /// </summary>
    public virtual DateTime? EndCreationTime { get; set; }
}
