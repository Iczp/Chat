using IczpNet.Chat.Enums;
using IczpNet.Pusher.Commands;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitSettingSimpleDto
{
    public virtual string MemberName { get; set; }

    /// <summary>
    /// 加入方式
    /// </summary>
    public virtual JoinWays? JoinWay { get; set; }

    /// <summary>
    /// 禁言过期时间
    /// </summary>
    public virtual DateTime? MuteExpireTime { get; set; }

    /// <summary>
    /// 邀请人
    /// </summary>
    public virtual Guid? InviterId { get; set; }

    public virtual bool IsPublic { get; set; }

    public virtual bool IsStatic { get; set; }

    public virtual bool IsCreator { get; set; }

    public virtual bool IsKilled { get; set; }

    public virtual bool IsInputEnabled { get; set; }

    public virtual bool IsEnabled { get; set; }

    /// <summary>
    /// 最后发言的消息
    /// </summary>
    public virtual long? LastSendMessageId { get; set; }

    /// <summary>
    /// 最后发言时间
    /// </summary>
    public virtual DateTime? LastSendTime { get; protected set; }
}
