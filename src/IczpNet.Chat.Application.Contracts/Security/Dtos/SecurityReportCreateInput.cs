using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Security.Dtos;

/// <summary>
/// 提交违规投诉/举报入参
/// </summary>
public class SecurityReportCreateInput
{
    /// <summary>
    /// 被投诉的目标对象 (URL 地址或消息内容摘要)
    /// </summary>
    [Required]
    [StringLength(1024)]
    public string Target { get; set; }

    /// <summary>
    /// 目标类型 (url, message, user)
    /// </summary>
    [StringLength(64)]
    public string TargetType { get; set; } = "url";

    /// <summary>
    /// 投诉原因 (诈骗, 色情, 违规引流, 赌博, 恶意木马等)
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Reason { get; set; }

    /// <summary>
    /// 详细说明/补充证据
    /// </summary>
    [StringLength(2000)]
    public string Description { get; set; }

    /// <summary>
    /// 关联的消息 ID
    /// </summary>
    public long? MessageId { get; set; }

    /// <summary>
    /// 举报人 ChatObjectId
    /// </summary>
    public long? ReporterChatObjectId { get; set; }

    /// <summary>
    /// 举报人 SessionUnitId
    /// </summary>
    public Guid? ReporterSessionUnitId { get; set; }
}
