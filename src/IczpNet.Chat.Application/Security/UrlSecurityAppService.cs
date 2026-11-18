using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Security.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Security;

/// <summary>
/// 链接安全与违规投诉应用服务
/// </summary>
public class UrlSecurityAppService : ChatAppService, IUrlSecurityAppService
{
    protected IUrlSecurityManager UrlSecurityManager { get; }
    protected IRepository<UrlSecurityRule, Guid> RuleRepository { get; }
    protected IRepository<SecurityReport, Guid> ReportRepository { get; }

    public UrlSecurityAppService(
        IUrlSecurityManager urlSecurityManager,
        IRepository<UrlSecurityRule, Guid> ruleRepository,
        IRepository<SecurityReport, Guid> reportRepository)
    {
        UrlSecurityManager = urlSecurityManager;
        RuleRepository = ruleRepository;
        ReportRepository = reportRepository;
    }

    /// <summary>
    /// 检测 URL 安全状态 (支持匿名与登录用户，结合 Redis 缓存)
    /// </summary>
    [AllowAnonymous]
    public virtual async Task<UrlCheckOutput> CheckUrlAsync(UrlCheckInput input)
    {
        var eval = await UrlSecurityManager.CheckUrlAsync(input.Url);
        return new UrlCheckOutput
        {
            Url = eval.Url,
            Domain = eval.Domain,
            Level = eval.Level,
            Category = eval.Category,
            Message = eval.Message,
            InterceptReason = eval.InterceptReason,
            Source = eval.Source
        };
    }

    /// <summary>
    /// 提交违规 URL 或消息举报
    /// </summary>
    public virtual async Task<SecurityReportDto> ReportAsync(SecurityReportCreateInput input)
    {
        var report = await UrlSecurityManager.SubmitReportAsync(
            target: input.Target,
            targetType: input.TargetType ?? "url",
            reason: input.Reason,
            description: input.Description,
            messageId: input.MessageId,
            reporterChatObjectId: input.ReporterChatObjectId,
            reporterSessionUnitId: input.ReporterSessionUnitId
        );

        return MapToReportDto(report);
    }

    /// <summary>
    /// 管理员指令：刷新风控规则至 Redis 缓存
    /// </summary>
    public virtual async Task RefreshCacheAsync()
    {
        await UrlSecurityManager.RefreshCacheAsync();
    }

    /// <summary>
    /// 分页查询 URL 安全规则列表
    /// </summary>
    public virtual async Task<PagedResultDto<UrlSecurityRuleDto>> GetRulesAsync(UrlSecurityRuleGetListInput input)
    {
        var query = await RuleRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var kw = input.Keyword.Trim().ToLower();
            query = query.Where(x => x.Host.Contains(kw) || (x.Reason != null && x.Reason.Contains(kw)));
        }

        if (input.Level.HasValue)
        {
            query = query.Where(x => x.Level == input.Level.Value);
        }

        if (input.Category.HasValue)
        {
            query = query.Where(x => x.Category == input.Category.Value);
        }

        if (input.IsEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == input.IsEnabled.Value);
        }

        var totalCount = await query.CountAsync();

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime desc" : input.Sorting;
        query = query.OrderBy(sorting);

        var list = await query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();

        var items = list.Select(MapToRuleDto).ToList();
        return new PagedResultDto<UrlSecurityRuleDto>(totalCount, items);
    }

    /// <summary>
    /// 获取单个 URL 安全规则
    /// </summary>
    public virtual async Task<UrlSecurityRuleDto> GetRuleAsync(Guid id)
    {
        var rule = await RuleRepository.GetAsync(id);
        return MapToRuleDto(rule);
    }

    /// <summary>
    /// 创建 URL 安全规则
    /// </summary>
    public virtual async Task<UrlSecurityRuleDto> CreateRuleAsync(UrlSecurityRuleCreateInput input)
    {
        var host = input.Host?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new UserFriendlyException("域名或匹配模式不能为空");
        }

        var exists = await (await RuleRepository.GetQueryableAsync()).AnyAsync(x => x.Host == host);
        if (exists)
        {
            throw new UserFriendlyException($"域名规则 '{host}' 已存在，请勿重复添加");
        }

        var rule = new UrlSecurityRule(
            id: GuidGenerator.Create(),
            host: host,
            level: input.Level,
            category: input.Category,
            reason: input.Reason)
        {
            IsEnabled = input.IsEnabled,
            ExpireTime = input.ExpireTime
        };

        await RuleRepository.InsertAsync(rule, autoSave: true);
        await UrlSecurityManager.RefreshCacheAsync();

        return MapToRuleDto(rule);
    }

    /// <summary>
    /// 更新 URL 安全规则
    /// </summary>
    public virtual async Task<UrlSecurityRuleDto> UpdateRuleAsync(Guid id, UrlSecurityRuleUpdateInput input)
    {
        var rule = await RuleRepository.GetAsync(id);
        var host = input.Host?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new UserFriendlyException("域名或匹配模式不能为空");
        }

        var duplicate = await (await RuleRepository.GetQueryableAsync()).AnyAsync(x => x.Host == host && x.Id != id);
        if (duplicate)
        {
            throw new UserFriendlyException($"域名规则 '{host}' 已存在");
        }

        rule.Host = host;
        rule.Level = input.Level;
        rule.Category = input.Category;
        rule.Reason = input.Reason;
        rule.IsEnabled = input.IsEnabled;
        rule.ExpireTime = input.ExpireTime;

        await RuleRepository.UpdateAsync(rule, autoSave: true);
        await UrlSecurityManager.RefreshCacheAsync();

        return MapToRuleDto(rule);
    }

    /// <summary>
    /// 删除 URL 安全规则
    /// </summary>
    public virtual async Task DeleteRuleAsync(Guid id)
    {
        await RuleRepository.DeleteAsync(id, autoSave: true);
        await UrlSecurityManager.RefreshCacheAsync();
    }

    /// <summary>
    /// 启停 URL 安全规则
    /// </summary>
    public virtual async Task SetRuleEnabledAsync(Guid id, bool isEnabled)
    {
        var rule = await RuleRepository.GetAsync(id);
        rule.IsEnabled = isEnabled;
        await RuleRepository.UpdateAsync(rule, autoSave: true);
        await UrlSecurityManager.RefreshCacheAsync();
    }

    /// <summary>
    /// 分页查询安全举报记录列表
    /// </summary>
    public virtual async Task<PagedResultDto<SecurityReportDto>> GetReportsAsync(SecurityReportGetListInput input)
    {
        var query = await ReportRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var kw = input.Keyword.Trim();
            query = query.Where(x =>
                x.TargetContent.Contains(kw) ||
                x.Reason.Contains(kw) ||
                (x.Description != null && x.Description.Contains(kw)));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.TargetType))
        {
            query = query.Where(x => x.TargetType == input.TargetType);
        }

        var totalCount = await query.CountAsync();
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime desc" : input.Sorting;
        query = query.OrderBy(sorting);

        var list = await query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();

        var items = list.Select(MapToReportDto).ToList();
        return new PagedResultDto<SecurityReportDto>(totalCount, items);
    }

    /// <summary>
    /// 审核处理安全举报记录
    /// </summary>
    public virtual async Task<SecurityReportDto> HandleReportAsync(Guid id, SecurityReportHandleInput input)
    {
        var report = await ReportRepository.GetAsync(id);
        report.Status = input.Status;
        report.AuditRemark = input.AuditRemark;

        if (input.AddBlacklist && !string.IsNullOrWhiteSpace(report.TargetContent))
        {
            try
            {
                var target = report.TargetContent.Trim();
                string domain = target;
                if (target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    if (Uri.TryCreate(target, UriKind.Absolute, out var uri))
                    {
                        domain = uri.Host.ToLowerInvariant();
                    }
                }

                var exists = await (await RuleRepository.GetQueryableAsync()).AnyAsync(x => x.Host == domain);
                if (!exists)
                {
                    var rule = new UrlSecurityRule(
                        id: GuidGenerator.Create(),
                        host: domain,
                        level: UrlSecurityLevel.Blocked,
                        category: UrlRiskCategory.Phishing,
                        reason: $"举报封禁 (工单ID: {report.Id:N}): {report.Reason}")
                    {
                        IsEnabled = true
                    };
                    await RuleRepository.InsertAsync(rule, autoSave: true);
                    await UrlSecurityManager.RefreshCacheAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "自动加黑名单处理异常: {Message}", ex.Message);
            }
        }

        await ReportRepository.UpdateAsync(report, autoSave: true);
        return MapToReportDto(report);
    }

    protected static UrlSecurityRuleDto MapToRuleDto(UrlSecurityRule entity)
    {
        return new UrlSecurityRuleDto
        {
            Id = entity.Id,
            Host = entity.Host,
            Level = entity.Level,
            Category = entity.Category,
            Reason = entity.Reason,
            IsEnabled = entity.IsEnabled,
            ExpireTime = entity.ExpireTime,
            HitCount = entity.HitCount,
            CreationTime = entity.CreationTime
        };
    }

    protected static SecurityReportDto MapToReportDto(SecurityReport entity)
    {
        return new SecurityReportDto
        {
            Id = entity.Id,
            TargetType = entity.TargetType,
            TargetContent = entity.TargetContent,
            Reason = entity.Reason,
            Description = entity.Description,
            MessageId = entity.MessageId,
            ReporterChatObjectId = entity.ReporterChatObjectId,
            ReporterSessionUnitId = entity.ReporterSessionUnitId,
            Status = entity.Status,
            AuditRemark = entity.AuditRemark,
            CreationTime = entity.CreationTime
        };
    }
}
