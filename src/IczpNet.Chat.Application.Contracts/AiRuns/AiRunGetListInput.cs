using IczpNet.Chat.BaseDtos;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.AiRuns;

[Serializable]
public class AiRunGetListInput : GetListInput
{
    /// <summary>
    /// 会话 Id
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// 发起方会话单元 Id
    /// </summary>
    public virtual Guid? RequesterSessionUnitId { get; set; }

    /// <summary>
    /// 提供商（如 Aurora）
    /// </summary>
    [MaxLength(64)]
    public virtual string Provider { get; set; }

    /// <summary>
    /// 运行状态
    /// </summary>
    public virtual AiRunStatus? Status { get; set; }

    /// <summary>
    /// 租约所有者 WorkerId
    /// </summary>
    [MaxLength(128)]
    public virtual string LeaseOwner { get; set; }

    /// <summary>
    /// 用户源消息 Id
    /// </summary>
    public virtual long? SourceMessageId { get; set; }

    /// <summary>
    /// 最终回复消息 Id
    /// </summary>
    public virtual long? OutputMessageId { get; set; }

    /// <summary>
    /// 创建时间起始
    /// </summary>
    public virtual DateTime? StartCreationTime { get; set; }

    /// <summary>
    /// 创建时间截止
    /// </summary>
    public virtual DateTime? EndCreationTime { get; set; }
}
