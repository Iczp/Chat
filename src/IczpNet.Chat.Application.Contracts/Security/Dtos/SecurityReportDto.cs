using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Security.Dtos;

/// <summary>
/// 违规举报记录 DTO
/// </summary>
public class SecurityReportDto : EntityDto<Guid>
{
    public string TargetType { get; set; }
    public string TargetContent { get; set; }
    public string Reason { get; set; }
    public string Description { get; set; }
    public long? MessageId { get; set; }
    public long? ReporterChatObjectId { get; set; }
    public Guid? ReporterSessionUnitId { get; set; }
    public int Status { get; set; }
    public string AuditRemark { get; set; }
    public DateTime CreationTime { get; set; }
}
