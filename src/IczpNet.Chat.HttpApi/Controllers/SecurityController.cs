using IczpNet.Chat.Security;
using IczpNet.Chat.Security.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp;

namespace IczpNet.Chat.Controllers;

/// <summary>
/// URL 安全风控与违规投诉控制器
/// </summary>
[Area(ChatRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ChatRemoteServiceConsts.RemoteServiceName)]
[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/security")]
public class SecurityController(IUrlSecurityAppService urlSecurityAppService) : ChatController, IUrlSecurityAppService
{
    protected IUrlSecurityAppService UrlSecurityAppService { get; } = urlSecurityAppService;

    /// <summary>
    /// 检测 URL 安全状态 (查 Redis 缓存 / 黑白名单规则评估)
    /// </summary>
    [HttpPost]
    [Route("check-url")]
    [AllowAnonymous]
    public virtual Task<UrlCheckOutput> CheckUrlAsync(UrlCheckInput input)
    {
        return UrlSecurityAppService.CheckUrlAsync(input);
    }

    /// <summary>
    /// 提交违规 URL 或消息举报
    /// </summary>
    [HttpPost]
    [Route("report")]
    public virtual Task<SecurityReportDto> ReportAsync(SecurityReportCreateInput input)
    {
        return UrlSecurityAppService.ReportAsync(input);
    }

    /// <summary>
    /// 管理员指令：刷新风控规则至 Redis 缓存
    /// </summary>
    [HttpPost]
    [Route("refresh-cache")]
    public virtual Task RefreshCacheAsync()
    {
        return UrlSecurityAppService.RefreshCacheAsync();
    }
}
