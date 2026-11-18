using IczpNet.Chat.Enums;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Security;

/// <summary>
/// URL 安全风控与违规举报领域服务实现
/// </summary>
public class UrlSecurityManager : DomainService, IUrlSecurityManager
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
}
