using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.ChatObjects.Dtos;
using System;

namespace IczpNet.Chat.SessionSections.SessionRequests.Dtos;

public class SessionRequestDto : BaseDto<Guid>
{
    /// <summary>
    /// 发起者Id
    /// </summary>
    public virtual long OwnerId { get; set; }

    /// <summary>
    /// 被请求者Id
    /// </summary>
    public virtual long? DestinationId { get; set; }

    /// <summary>
    /// 发起者
    /// </summary>
    public virtual ChatObjectSimpleDto Owner { get; set; }

    /// <summary>
    /// 被请求者
    /// </summary>
    public virtual ChatObjectSimpleDto Destination { get; set; }

    /// <summary>
    /// 是否处理过
    /// </summary>
    public virtual bool IsHandled { get; set; }

    /// <summary>
    /// 是否同意
    /// </summary>
    public virtual bool? IsAgreed { get; set; }

    /// <summary>
    /// 请求消息
    /// </summary>
    public virtual string RequestMessage { get; set; }

    /// <summary>
    /// 是否过期
    /// </summary>
    public virtual bool IsExpired { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public virtual DateTime? ExpirationTime { get; set; }

    /// <summary>
    /// 处理消息
    /// </summary>
    public virtual string HandleMessage { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    public virtual DateTime? HandleTime { get; set; }

}
