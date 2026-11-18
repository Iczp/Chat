using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionUnitSettings.Dtos;

public class SessionUnitMemberSettingDto
{
    /// <summary>
    /// 会话单元Id
    /// </summary>
    public virtual Guid SessionUnitId { get; set; }

    /// <summary>
    /// 会话内的名称
    /// </summary>
    public virtual string MemberName { get; set; }

    /// <summary>
    /// 加入方式
    /// </summary>
    public virtual JoinWays? JoinWay { get; set; }

    /// <summary>
    /// 加入时间
    /// </summary>
    public virtual DateTime? JoinTime { get; set; }

    /// <summary>
    /// 加入方式
    /// </summary>
    public virtual string JoinWayDescription { get; set; }

    /// <summary>
    /// 最后发送时间
    /// </summary>
    public DateTime? LastSendTime { get; set; }

    /// <summary>
    /// 最后发送消息Id
    /// </summary>
    public virtual long? LastSendMessageId { get; set; }

    /// <summary>
    /// 是否固定成员
    /// </summary>
    public virtual bool IsStatic { get; set; } 

    /// <summary>
    /// 是否公有成员
    /// </summary>
    public virtual bool IsPublic { get; set; } 

    /// <summary>
    /// 是否可见的
    /// </summary>
    public virtual bool IsVisible { get; set; }

    /// <summary>
    /// 是否可用
    /// </summary>
    public virtual bool IsEnabled { get; set; }

    /// <summary>
    /// 是否创建者（群主等）
    /// </summary>
    public virtual bool IsCreator { get; set; } 

    /// <summary>
    /// 禁言过期时间
    /// </summary>
    public virtual DateTime? MuteExpireTime { get; set; }
}
