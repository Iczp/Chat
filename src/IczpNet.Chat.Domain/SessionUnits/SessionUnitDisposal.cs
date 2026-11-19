using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.Enums;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话单元处置与删除记录
/// </summary>
[Comment("会话单元处置与删除记录")]
public class SessionUnitDisposal : CreationAuditedEntity<long>, IMultiTenant
{
    /// <summary>
    /// 租户 ID
    /// </summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// 所属聊天对象（用户/ChatObject），客户端拉取时的分区键
    /// </summary>
    [Required]
    [Comment("所属聊天对象ID")]
    public virtual long OwnerId { get; protected set; }

    /// <summary>
    /// 会话单元 ID
    /// </summary>
    [Required]
    [Comment("会话单元ID")]
    public virtual Guid SessionUnitId { get; protected set; }

    /// <summary>
    /// 所属会话 ID
    /// </summary>
    [Comment("所属会话ID")]
    public virtual Guid? SessionId { get; protected set; }

    /// <summary>
    /// 对方聊天对象 ID（好友ID 或 群ID）
    /// </summary>
    [Comment("对方聊天对象ID")]
    public virtual long? DestinationId { get; protected set; }

    /// <summary>
    /// 处置动作类型: 1=Remove(移出留记录), 2=Delete(软删除清理), 3=Hide(不显示), 4=ClearMessage(清消息)
    /// </summary>
    [Required]
    [Comment("处置动作类型: 1=Remove, 2=Delete, 3=Hide, 4=ClearMessage")]
    public virtual SessionUnitDisposalType DisposalType { get; protected set; }

    /// <summary>
    /// 处置原因细分
    /// </summary>
    [Required]
    [Comment("处置原因细分")]
    public virtual SessionUnitDisposalReason Reason { get; protected set; }

    /// <summary>
    /// 操作执行人ID（例如管理员ID 或 自身）
    /// </summary>
    [Comment("操作执行人ID")]
    public virtual long? OperatorId { get; protected set; }

    /// <summary>
    /// 附加说明
    /// </summary>
    [MaxLength(256)]
    [Comment("附加说明")]
    public virtual string Description { get; protected set; }

    protected SessionUnitDisposal() { }

    public SessionUnitDisposal(
        long ownerId,
        Guid sessionUnitId,
        Guid? sessionId,
        long? destinationId,
        SessionUnitDisposalType disposalType,
        SessionUnitDisposalReason reason,
        long? operatorId = null,
        string description = null,
        Guid? tenantId = null)
    {
        OwnerId = ownerId;
        SessionUnitId = sessionUnitId;
        SessionId = sessionId;
        DestinationId = destinationId;
        DisposalType = disposalType;
        Reason = reason;
        OperatorId = operatorId;
        Description = description;
        TenantId = tenantId;
    }
}
