using IczpNet.Chat.Security.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Security;

/// <summary>
/// 链接安全与违规投诉应用服务接口
/// </summary>
public interface IUrlSecurityAppService : IApplicationService
{
    /// <summary>
    /// 检测 URL 安全状态 (命中 Redis 缓存或执行风控规则评估)
    /// </summary>
    Task<UrlCheckOutput> CheckUrlAsync(UrlCheckInput input);

    /// <summary>
    /// 提交违规 URL 或消息举报
    /// </summary>
    Task<SecurityReportDto> ReportAsync(SecurityReportCreateInput input);

    /// <summary>
    /// 管理员指令：刷新风控规则至 Redis 缓存
    /// </summary>
    Task RefreshCacheAsync();

    /// <summary>
    /// 分页查询 URL 安全规则列表
    /// </summary>
    Task<PagedResultDto<UrlSecurityRuleDto>> GetRulesAsync(UrlSecurityRuleGetListInput input);

    /// <summary>
    /// 获取单个 URL 安全规则
    /// </summary>
    Task<UrlSecurityRuleDto> GetRuleAsync(Guid id);

    /// <summary>
    /// 创建 URL 安全规则
    /// </summary>
    Task<UrlSecurityRuleDto> CreateRuleAsync(UrlSecurityRuleCreateInput input);

    /// <summary>
    /// 更新 URL 安全规则
    /// </summary>
    Task<UrlSecurityRuleDto> UpdateRuleAsync(Guid id, UrlSecurityRuleUpdateInput input);

    /// <summary>
    /// 删除 URL 安全规则
    /// </summary>
    Task DeleteRuleAsync(Guid id);

    /// <summary>
    /// 启停 URL 安全规则
    /// </summary>
    Task SetRuleEnabledAsync(Guid id, bool isEnabled);

    /// <summary>
    /// 分页查询安全举报记录列表
    /// </summary>
    Task<PagedResultDto<SecurityReportDto>> GetReportsAsync(SecurityReportGetListInput input);

    /// <summary>
    /// 审核处理安全举报记录
    /// </summary>
    Task<SecurityReportDto> HandleReportAsync(Guid id, SecurityReportHandleInput input);
}
