using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Security.Dtos;

/// <summary>
/// URL 安全规则列表查询参数
/// </summary>
public class UrlSecurityRuleGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }
    public UrlSecurityLevel? Level { get; set; }
    public UrlRiskCategory? Category { get; set; }
    public bool? IsEnabled { get; set; }
}
