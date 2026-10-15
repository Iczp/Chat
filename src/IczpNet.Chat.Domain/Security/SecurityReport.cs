using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Security;

/// <summary>
/// 安全违规投诉举报记录表
/// </summary>
[Comment("违规投诉举报记录表")]
[Index(nameof(TargetType))]
[Index(nameof(Status))]
[Index(nameof(CreationTime), AllDescending = true)]
public class SecurityReport : BaseEntity<Guid>
{
    /// <summary>
    /// 举报人 ChatObjectId
    /// </summary>
    [Comment("举报人 ChatObjectId")]
    public virtual long? ReporterChatObjectId { get; set; }

    /// <summary>
    /// 举报人 SessionUnitId
    /// </summary>
    [Comment("举报人 SessionUnitId")]
    public virtual Guid? ReporterSessionUnitId { get; set; }

    /// <summary>
    /// 投诉目标类型：url / message / user
    /// </summary>
    [Comment("投诉目标类型：url / message / user")]
    [Required]
    [MaxLength(32)]
    public virtual string TargetType { get; set; }

    /// <summary>
    /// 投诉目标内容（URL 链接 / 消息文本摘录）
    /// </summary>
    [Comment("投诉目标内容")]
    [Required]
    [MaxLength(1024)]
    public virtual string TargetContent { get; set; }

    /// <summary>
    /// 关联的消息唯一ID
    /// </summary>
    [Comment("关联消息ID")]
    public virtual long? MessageId { get; set; }

    /// <summary>
    /// 投诉原因
    /// </summary>
    [Comment("投诉原因")]
    [Required]
    [MaxLength(128)]
    public virtual string Reason { get; set; }

    /// <summary>
    /// 用户填写的详细描述或受骗情况
    /// </summary>
    [Comment("详细描述")]
    [MaxLength(1000)]
    public virtual string Description { get; set; }

    /// <summary>
    /// 处理状态：0=待审核, 1=已确认并封禁, 2=驳回/忽略
    /// </summary>
    [Comment("处理状态：0=待审核, 1=已确认并封禁, 2=驳回/忽略")]
    public virtual int Status { get; set; }

    /// <summary>
    /// 审核人员意见
    /// </summary>
    [Comment("审核意见")]
    [MaxLength(512)]
    public virtual string AuditRemark { get; set; }

    protected SecurityReport() { }

    public SecurityReport(
        Guid id,
        string targetType,
        string targetContent,
        string reason,
        string description = null,
        long? messageId = null)
        : base(id)
    {
        TargetType = targetType;
        TargetContent = targetContent;
        Reason = reason;
        Description = description;
        MessageId = messageId;
        Status = 0;
    }
}
