using IczpNet.Chat.Security.Dtos;
using System.Threading.Tasks;
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
}
