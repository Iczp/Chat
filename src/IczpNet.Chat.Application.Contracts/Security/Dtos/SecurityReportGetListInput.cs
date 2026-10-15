using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Security.Dtos;

/// <summary>
/// 举报列表查询参数
/// </summary>
public class SecurityReportGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }
    public int? Status { get; set; }
    public string TargetType { get; set; }
}
