using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Security.Dtos;

/// <summary>
/// 举报审核处理入参
/// </summary>
public class SecurityReportHandleInput
{
    /// <summary>
    /// 处理结果：1=已确认并封禁, 2=驳回/忽略
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// 审核意见或处置备注
    /// </summary>
    [MaxLength(512)]
    public string AuditRemark { get; set; }

    /// <summary>
    /// 是否同时将该域名加入黑名单规则
    /// </summary>
    public bool AddBlacklist { get; set; } = true;
}
