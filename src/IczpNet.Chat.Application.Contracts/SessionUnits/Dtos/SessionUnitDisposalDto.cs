using System;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionUnits.Dtos;

/// <summary>
/// 会话单元处置与删除记录 DTO
/// </summary>
public class SessionUnitDisposalDto : EntityDto<long>
{
    /// <summary>
    /// 所属聊天对象 ID
    /// </summary>
    public virtual long OwnerId { get; set; }

    /// <summary>
    /// 会话单元 ID
    /// </summary>
    public virtual Guid SessionUnitId { get; set; }

    /// <summary>
    /// 会话 ID
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// 对方聊天对象 ID（好友ID 或 群ID）
    /// </summary>
    public virtual long? DestinationId { get; set; }

    /// <summary>
    /// 处置类型: 1=Remove(移出留记录), 2=Delete(彻底软删), 3=Hide(隐藏), 4=ClearMessage(清消息)
    /// </summary>
    public virtual SessionUnitDisposalType DisposalType { get; set; }

    /// <summary>
    /// 处置原因细分
    /// </summary>
    public virtual SessionUnitDisposalReason Reason { get; set; }

    /// <summary>
    /// 操作人 ID
    /// </summary>
    public virtual long? OperatorId { get; set; }

    /// <summary>
    /// 附加说明
    /// </summary>
    public virtual string Description { get; set; }

    /// <summary>
    /// 处置发生时间
    /// </summary>
    public virtual DateTime CreationTime { get; set; }
}
