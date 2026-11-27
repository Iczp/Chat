using IczpNet.Chat.Enums;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Security;

/// <summary>
/// URL 安全风控与违规举报领域服务实现
/// </summary>
[ExposeServices(typeof(UrlSecurityManager), typeof(IUrlSecurityManager), typeof(IUrlSecurityAdminManager))]
public class UrlSecurityManager : DomainService, IUrlSecurityAdminManager
{
    private readonly IRepository<UrlSecurityRule, Guid> _ruleRepository;
    private readonly IRepository<SecurityReport, Guid> _reportRepository;

    protected IConnectionMultiplexer ConnectionMultiplexer =>
        LazyServiceProvider.LazyGetRequiredService<IConnectionMultiplexer>();

    protected IDatabase Database => ConnectionMultiplexer.GetDatabase();

    public const string WhitelistKey = "Chat:Security:Whitelist";
    public const string BlacklistKey = "Chat:Security:Blacklist";
    public const string VersionKey = "Chat:Security:Version";
    public const string CachePrefix = "Chat:Security:Cache:";

    public UrlSecurityManager(
        IRepository<UrlSecurityRule, Guid> ruleRepository,
        IRepository<SecurityReport, Guid> reportRepository)
    {
        _ruleRepository = ruleRepository;
        _reportRepository = reportRepository;
    }

    public virtual async Task<UrlSecurityEvaluationResult> CheckUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return new UrlSecurityEvaluationResult
            {
                Url = url,
                Level = UrlSecurityLevel.Blocked,
                Message = "链接地址为空"
            };
        }

        var normalizedUrl = url.Trim();
        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
        {
            return new UrlSecurityEvaluationResult
            {
                Url = normalizedUrl,
                Level = UrlSecurityLevel.Blocked,
                Message = "无法识别的非法链接"
            };
        }

        var domain = uri.Host.ToLowerInvariant();
        var urlHash = ComputeHash(normalizedUrl);

        // 1. 优先检查黑名单规则 (Redis Hash，O(1) 毫秒级)
        // 支持自身及主域名通配匹配 (例如 a.b.evil.com 匹配 evil.com 与 *.evil.com)
        try
        {
            foreach (var candidate in GetDomainCandidates(domain))
            {
                var blackReason = await Database.HashGetAsync(BlacklistKey, candidate);
                if (blackReason.HasValue)
                {
                    return new UrlSecurityEvaluationResult
                    {
                        Url = normalizedUrl,
                        Domain = domain,
                        Level = UrlSecurityLevel.Blocked,
                        Category = UrlRiskCategory.Fraud,
                        Message = "据用户投诉及安全风控库核查，该网址包含违法/诈骗/违规内容，已全面停止访问。",
                        InterceptReason = blackReason.ToString(),
                        Source = "redis_blacklist"
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[UrlSecurityManager] Redis 读取黑名单异常");
        }

        // 2. 优先检查白名单规则 (Redis Set，O(1) 毫秒级)
        // 确保后台只要添加或更新白名单，下一次点击 100% 毫秒级直接命中放行，不受任何历史缓存干扰
        try
        {
            foreach (var candidate in GetDomainCandidates(domain))
            {
                var isWhite = await Database.SetContainsAsync(WhitelistKey, candidate);
                if (isWhite)
                {
                    return new UrlSecurityEvaluationResult
                    {
                        Url = normalizedUrl,
                        Domain = domain,
                        Level = UrlSecurityLevel.Safe,
                        Category = UrlRiskCategory.OfficialOrWhitelisted,
                        Message = "官方/受信安全网站",
                        Source = "redis_whitelist"
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[UrlSecurityManager] Redis 读取白名单异常");
        }

        // 3. 黑白名单均未命中时，检查带版本号的 URL 临时评估缓存
        string cacheKey = null;
        try
        {
            var version = await Database.StringGetAsync(VersionKey);
            cacheKey = $"{CachePrefix}{version}:{urlHash}";
            var cached = await Database.StringGetAsync(cacheKey);
            if (cached.HasValue)
            {
                var r = JsonSerializer.Deserialize<UrlSecurityEvaluationResult>((string)cached!);
                if (r != null)
                {
                    r.Source = "redis_cache";
                    return r;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[UrlSecurityManager] Redis 读取缓存异常");
        }

        // 4. 兜底策略：未知普通第三方外链（灰名单），返回警示提示
        var warningResult = new UrlSecurityEvaluationResult
        {
            Url = normalizedUrl,
            Domain = domain,
            Level = UrlSecurityLevel.Warning,
            Category = UrlRiskCategory.UnknownThirdParty,
            Message = "您即将离开本应用访问第三方外链，请注意防范电信网络诈骗，保护个人信息与财产安全。",
            InterceptReason = "非平台官方域名，未经验证的第三方链接",
            Source = "backend_engine"
        };

        if (!string.IsNullOrEmpty(cacheKey))
        {
            await SetCacheAsync(cacheKey, warningResult);
        }

        return warningResult;
    }

    public virtual async Task<SecurityReport> SubmitReportAsync(
        string target,
        string targetType,
        string reason,
        string description = null,
        long? messageId = null,
        long? reporterChatObjectId = null,
        Guid? reporterSessionUnitId = null)
    {
        var report = new SecurityReport(
            GuidGenerator.Create(),
            targetType,
            target,
            reason,
            description,
            messageId
        )
        {
            ReporterChatObjectId = reporterChatObjectId,
            ReporterSessionUnitId = reporterSessionUnitId
        };

        await _reportRepository.InsertAsync(report, autoSave: true);
        Logger.LogInformation($"[SecurityReport] 收到新投诉 -> Type: {targetType}, Target: {target}, Reason: {reason}");

        return report;
    }

    /// <summary>
    /// 全量同步启用的规则至 Redis，并递增版本号使历史 URL 结果缓存立竿见影失效
    /// </summary>
    public virtual async Task RefreshCacheAsync()
    {
        var rules = await _ruleRepository.GetListAsync(x => x.IsEnabled);

        try
        {
            var batch = Database.CreateBatch();
            var t1 = batch.KeyDeleteAsync(WhitelistKey);
            var t2 = batch.KeyDeleteAsync(BlacklistKey);
            var t3 = batch.StringIncrementAsync(VersionKey);

            foreach (var rule in rules)
            {
                var host = rule.Host?.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(host)) continue;

                if (rule.Level == UrlSecurityLevel.Safe)
                {
                    _ = batch.SetAddAsync(WhitelistKey, host);
                }
                else if (rule.Level == UrlSecurityLevel.Blocked)
                {
                    _ = batch.HashSetAsync(BlacklistKey, host, rule.Reason ?? "违规阻断");
                }
            }

            batch.Execute();
            await Task.WhenAll(t1, t2, t3);
            Logger.LogInformation($"[UrlSecurity] 刷新 Redis 规则完成，共同步 {rules.Count} 条规则，版本号已递增");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[UrlSecurity] 刷新 Redis 规则失败");
        }
    }

    private async Task SetCacheAsync(string cacheKey, UrlSecurityEvaluationResult result)
    {
        try
        {
            await Database.StringSetAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                TimeSpan.FromHours(24)
            );
        }
        catch { }
    }

    /// <summary>
    /// 获取域名的候选匹配项（包含完整域名、泛域名及父级域名）
    /// 例如: open.api.gotoim.com 会产生:
    /// 1. open.api.gotoim.com
    /// 2. *.open.api.gotoim.com
    /// 3. api.gotoim.com
    /// 4. *.api.gotoim.com
    /// 5. gotoim.com
    /// 6. *.gotoim.com
    /// </summary>
    private static IEnumerable<string> GetDomainCandidates(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain)) yield break;

        yield return domain;
        yield return "*." + domain;

        var parts = domain.Split('.');
        for (int i = 1; i < parts.Length - 1; i++)
        {
            var parent = string.Join('.', parts.Skip(i));
            yield return parent;
            yield return "*." + parent;
        }
    }

    private static string ComputeHash(string input)
    {
        using var md5 = MD5.Create();
        var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }

    public virtual async Task<UrlSecurityRuleAdminListResult> GetRulesAdminListAsync(
        string keyword = null,
        UrlSecurityLevel? level = null,
        UrlRiskCategory? category = null,
        bool? isEnabled = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = (await _ruleRepository.GetQueryableAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x =>
                x.Host.Contains(keyword.Trim().ToLowerInvariant()) ||
                (x.Reason != null && x.Reason.Contains(keyword.Trim())))
            .WhereIf(level.HasValue, x => x.Level == level.Value)
            .WhereIf(category.HasValue, x => x.Category == category.Value)
            .WhereIf(isEnabled.HasValue, x => x.IsEnabled == isEnabled.Value);

        var totalCount = await AsyncExecuter.LongCountAsync(queryable, cancellationToken);
        if (totalCount == 0)
        {
            return new UrlSecurityRuleAdminListResult();
        }

        queryable = ApplyRuleSorting(queryable, sorting);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount, 1, 200)),
            cancellationToken);

        return new UrlSecurityRuleAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<UrlSecurityRule> GetRuleAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _ruleRepository.FindAsync(id, cancellationToken: cancellationToken);
        return entity ?? throw new EntityNotFoundException(typeof(UrlSecurityRule), id);
    }

    public virtual async Task<UrlSecurityRule> CreateRuleAdminAsync(
        string host,
        UrlSecurityLevel level,
        UrlRiskCategory category,
        string reason,
        bool isEnabled,
        DateTime? expireTime,
        CancellationToken cancellationToken = default)
    {
        var normalizedHost = host?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedHost))
        {
            throw new BusinessException("ChatAdmin:UrlSecurityHostRequired", "域名模式不能为空。");
        }

        if (await _ruleRepository.AnyAsync(x => x.Host == normalizedHost, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:DuplicateUrlSecurityHost", $"域名规则 [{normalizedHost}] 已存在。");
        }

        var rule = new UrlSecurityRule(GuidGenerator.Create(), normalizedHost, level, category, reason)
        {
            IsEnabled = isEnabled,
            ExpireTime = expireTime
        };

        await _ruleRepository.InsertAsync(rule, autoSave: true, cancellationToken: cancellationToken);
        await RefreshCacheAsync();
        return rule;
    }

    public virtual async Task<UrlSecurityRule> UpdateRuleAdminAsync(
        Guid id,
        string host,
        UrlSecurityLevel level,
        UrlRiskCategory category,
        string reason,
        bool isEnabled,
        DateTime? expireTime,
        CancellationToken cancellationToken = default)
    {
        var rule = await _ruleRepository.GetAsync(id, cancellationToken: cancellationToken);
        var normalizedHost = host?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedHost))
        {
            throw new BusinessException("ChatAdmin:UrlSecurityHostRequired", "域名模式不能为空。");
        }

        if (await _ruleRepository.AnyAsync(x => x.Id != id && x.Host == normalizedHost, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:DuplicateUrlSecurityHost", $"域名规则 [{normalizedHost}] 已存在。");
        }

        rule.Host = normalizedHost;
        rule.Level = level;
        rule.Category = category;
        rule.Reason = reason;
        rule.IsEnabled = isEnabled;
        rule.ExpireTime = expireTime;

        await _ruleRepository.UpdateAsync(rule, autoSave: true, cancellationToken: cancellationToken);
        await RefreshCacheAsync();
        return rule;
    }

    public virtual async Task<UrlSecurityRule> SetRuleEnabledAdminAsync(
        Guid id,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        var rule = await _ruleRepository.GetAsync(id, cancellationToken: cancellationToken);
        rule.IsEnabled = isEnabled;
        await _ruleRepository.UpdateAsync(rule, autoSave: true, cancellationToken: cancellationToken);
        await RefreshCacheAsync();
        return rule;
    }

    public virtual async Task DeleteRuleAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var rule = await _ruleRepository.FindAsync(id, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException(typeof(UrlSecurityRule), id);

        await _ruleRepository.DeleteAsync(rule, autoSave: true, cancellationToken: cancellationToken);
        await RefreshCacheAsync();
    }

    public virtual async Task<SecurityReportAdminListResult> GetReportsAdminListAsync(
        string keyword = null,
        int? status = null,
        string targetType = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = (await _reportRepository.GetQueryableAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x =>
                x.TargetContent.Contains(keyword.Trim()) ||
                x.Reason.Contains(keyword.Trim()) ||
                (x.Description != null && x.Description.Contains(keyword.Trim())))
            .WhereIf(status.HasValue, x => x.Status == status.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(targetType), x => x.TargetType == targetType.Trim());

        var totalCount = await AsyncExecuter.LongCountAsync(queryable, cancellationToken);
        if (totalCount == 0)
        {
            return new SecurityReportAdminListResult();
        }

        queryable = ApplyReportSorting(queryable, sorting);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount, 1, 200)),
            cancellationToken);

        return new SecurityReportAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<SecurityReport> GetReportAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _reportRepository.FindAsync(id, cancellationToken: cancellationToken);
        return entity ?? throw new EntityNotFoundException(typeof(SecurityReport), id);
    }

    public virtual async Task<SecurityReport> HandleReportAdminAsync(
        Guid id,
        int status,
        string auditRemark,
        bool addBlacklist,
        CancellationToken cancellationToken = default)
    {
        if (status is not (1 or 2))
        {
            throw new BusinessException("ChatAdmin:InvalidSecurityReportStatus", "处理状态必须为已确认或已驳回。");
        }

        var report = await _reportRepository.GetAsync(id, cancellationToken: cancellationToken);
        if (report.Status != 0)
        {
            throw new BusinessException("ChatAdmin:SecurityReportAlreadyHandled", "该举报工单已处理，不可重复审核。");
        }

        report.Status = status;
        report.AuditRemark = auditRemark;
        await _reportRepository.UpdateAsync(report, autoSave: true, cancellationToken: cancellationToken);

        if (status == 1 && addBlacklist && !string.IsNullOrWhiteSpace(report.TargetContent))
        {
            var host = Uri.TryCreate(report.TargetContent.Trim(), UriKind.Absolute, out var uri)
                ? uri.Host.ToLowerInvariant()
                : report.TargetContent.Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(host) && host.Length <= 256)
            {
                var existingRule = await _ruleRepository.FindAsync(x => x.Host == host, cancellationToken: cancellationToken);
                if (existingRule == null)
                {
                    await _ruleRepository.InsertAsync(
                        new UrlSecurityRule(
                            GuidGenerator.Create(),
                            host,
                            UrlSecurityLevel.Blocked,
                            UrlRiskCategory.Other,
                            string.IsNullOrWhiteSpace(auditRemark) ? report.Reason : auditRemark),
                        autoSave: true,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    existingRule.Level = UrlSecurityLevel.Blocked;
                    existingRule.IsEnabled = true;
                    if (!string.IsNullOrWhiteSpace(auditRemark))
                    {
                        existingRule.Reason = auditRemark;
                    }
                    await _ruleRepository.UpdateAsync(existingRule, autoSave: true, cancellationToken: cancellationToken);
                }
                await RefreshCacheAsync();
            }
        }

        return report;
    }

    private static IQueryable<UrlSecurityRule> ApplyRuleSorting(IQueryable<UrlSecurityRule> queryable, string sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return queryable.OrderByDescending(x => x.CreationTime);
        }

        var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return field switch
        {
            "host" => desc ? queryable.OrderByDescending(x => x.Host) : queryable.OrderBy(x => x.Host),
            "level" => desc ? queryable.OrderByDescending(x => x.Level) : queryable.OrderBy(x => x.Level),
            "hitcount" => desc ? queryable.OrderByDescending(x => x.HitCount) : queryable.OrderBy(x => x.HitCount),
            "creationtime" => desc ? queryable.OrderByDescending(x => x.CreationTime) : queryable.OrderBy(x => x.CreationTime),
            _ => queryable.OrderByDescending(x => x.CreationTime)
        };
    }

    private static IQueryable<SecurityReport> ApplyReportSorting(IQueryable<SecurityReport> queryable, string sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return queryable.OrderByDescending(x => x.CreationTime);
        }

        var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return field switch
        {
            "status" => desc ? queryable.OrderByDescending(x => x.Status) : queryable.OrderBy(x => x.Status),
            "targettype" => desc ? queryable.OrderByDescending(x => x.TargetType) : queryable.OrderBy(x => x.TargetType),
            "creationtime" => desc ? queryable.OrderByDescending(x => x.CreationTime) : queryable.OrderBy(x => x.CreationTime),
            _ => queryable.OrderByDescending(x => x.CreationTime)
        };
    }
}
