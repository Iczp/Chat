using System;

namespace IczpNet.Chat.Locations;


public class SessionLocationCacheItem 
{
    /// <summary>
    /// 会话单元
    /// </summary>
    public Guid SessionUnitId { get; set; }

    /// <summary>
    /// 聊天对象Id
    /// </summary>
    public long? ChatObjectId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 最近活动时间
    /// </summary>
    public DateTime ActiveTime { get; set; } = DateTime.Now;

}
