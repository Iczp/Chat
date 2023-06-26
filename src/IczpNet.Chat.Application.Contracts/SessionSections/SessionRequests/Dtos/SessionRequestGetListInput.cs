using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.SessionRequests.Dtos;

public class SessionRequestGetListInput : GetListInput
{
    /// <summary>
    /// 所属聊天对象Id
    /// </summary>
    public virtual long? OwnerId { get; set; }

    /// <summary>
    /// 目标聊天对象Id
    /// </summary>
    public virtual long? DestinationId { get; set; }

    /// <summary>
    /// 是否可用
    /// </summary>
    public virtual bool? IsEnabled { get; set; }

    /// <summary>
    /// 是否过期
    /// </summary>
    public virtual bool? IsExpired { get; set; }

    /// <summary>
    /// 是否处理过
    /// </summary>
    public virtual bool? IsHandled { get; set; }

    /// <summary>
    /// 是否同意请求
    /// </summary>
    public virtual bool? IsAgreed { get; set; }

    /// <summary>
    /// 处理起始时间
    /// </summary>
    public virtual DateTime? StartHandleTime { get; set; }

    /// <summary>
    /// 处理结束时间
    /// </summary>
    public virtual DateTime? EndHandleTime { get; set; }

    /// <summary>
    /// 创建请求的起始时间
    /// </summary>
    public virtual DateTime? StartCreationTime { get; set; }

    /// <summary>
    /// 创建请求的结束时间
    /// </summary>
    public virtual DateTime? EndCreationTime { get; set; }
}
