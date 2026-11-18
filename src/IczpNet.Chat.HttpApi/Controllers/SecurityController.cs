using IczpNet.Chat.Security;
using IczpNet.Chat.Security.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

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

    /// <summary>
    /// 分页查询 URL 安全规则列表
    /// </summary>
    [HttpGet]
    [Route("rules")]
    public virtual Task<PagedResultDto<UrlSecurityRuleDto>> GetRulesAsync([FromQuery] UrlSecurityRuleGetListInput input)
    {
        return UrlSecurityAppService.GetRulesAsync(input);
    }

    /// <summary>
    /// 获取单个 URL 安全规则
    /// </summary>
    [HttpGet]
    [Route("rule/{id}")]
    public virtual Task<UrlSecurityRuleDto> GetRuleAsync(Guid id)
    {
        return UrlSecurityAppService.GetRuleAsync(id);
    }

    /// <summary>
    /// 创建 URL 安全规则
    /// </summary>
    [HttpPost]
    [Route("rule")]
    public virtual Task<UrlSecurityRuleDto> CreateRuleAsync(UrlSecurityRuleCreateInput input)
    {
        return UrlSecurityAppService.CreateRuleAsync(input);
    }

    /// <summary>
    /// 更新 URL 安全规则
    /// </summary>
    [HttpPut]
    [Route("rule/{id}")]
    public virtual Task<UrlSecurityRuleDto> UpdateRuleAsync(Guid id, UrlSecurityRuleUpdateInput input)
    {
        return UrlSecurityAppService.UpdateRuleAsync(id, input);
    }

    /// <summary>
    /// 删除 URL 安全规则
    /// </summary>
    [HttpDelete]
    [Route("rule/{id}")]
    public virtual Task DeleteRuleAsync(Guid id)
    {
        return UrlSecurityAppService.DeleteRuleAsync(id);
    }

    /// <summary>
    /// 启停 URL 安全规则
    /// </summary>
    [HttpPost]
    [Route("rule/{id}/set-enabled")]
    public virtual Task SetRuleEnabledAsync(Guid id, [FromQuery] bool isEnabled)
    {
        return UrlSecurityAppService.SetRuleEnabledAsync(id, isEnabled);
    }

    /// <summary>
    /// 分页查询安全举报记录列表
    /// </summary>
    [HttpGet]
    [Route("reports")]
    public virtual Task<PagedResultDto<SecurityReportDto>> GetReportsAsync([FromQuery] SecurityReportGetListInput input)
    {
        return UrlSecurityAppService.GetReportsAsync(input);
    }

    /// <summary>
    /// 审核处理安全举报记录
    /// </summary>
    [HttpPost]
    [Route("report/{id}/handle")]
    public virtual Task<SecurityReportDto> HandleReportAsync(Guid id, SecurityReportHandleInput input)
    {
        return UrlSecurityAppService.HandleReportAsync(id, input);
    }
}
