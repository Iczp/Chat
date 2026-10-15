using IczpNet.Chat.Enums;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
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

        var domain = uri.Host.ToLower();
        var urlHash = ComputeHash(normalizedUrl);
        var cacheKey = $"{CachePrefix}{urlHash}";

        // 1. 优先查 Redis 结果缓存 (TTL 24h)
        try
        {
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
            Logger.LogWarning(ex, "[UrlSecurityManager] Redis 读取缓存异常，回退本地匹配");
        }

        // 2. 检查黑名单规则 (Redis Hash)
        try
        {
            var blackReason = await Database.HashGetAsync(BlacklistKey, domain);
            if (blackReason.HasValue)
            {
                var result = new UrlSecurityEvaluationResult
                {
                    Url = normalizedUrl,
                    Domain = domain,
                    Level = UrlSecurityLevel.Blocked,
                    Category = UrlRiskCategory.Fraud,
                    Message = "据用户投诉及安全风控库核查，该网址包含违法/诈骗/违规内容，已全面停止访问。",
                    InterceptReason = blackReason.ToString(),
                    Source = "redis_blacklist"
                };
                await SetCacheAsync(cacheKey, result);
                return result;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[UrlSecurityManager] Redis 读取黑名单异常");
        }

        // 3. 检查白名单规则 (Redis Set)
        try
        {
            var isWhite = await Database.SetContainsAsync(WhitelistKey, domain);
            if (isWhite)
            {
                var result = new UrlSecurityEvaluationResult
                {
                    Url = normalizedUrl,
                    Domain = domain,
                    Level = UrlSecurityLevel.Safe,
                    Category = UrlRiskCategory.OfficialOrWhitelisted,
                    Message = "官方/受信安全网站",
                    Source = "redis_whitelist"
                };
                await SetCacheAsync(cacheKey, result);
                return result;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "[UrlSecurityManager] Redis 读取白名单异常");
        }

        // 4. 兜底策略：未知普通第三方外链（灰名单）
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

        await SetCacheAsync(cacheKey, warningResult);
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

    public virtual async Task RefreshCacheAsync()
    {
        var rules = await _ruleRepository.GetListAsync(x => x.IsEnabled);

        try
        {
            var batch = Database.CreateBatch();
            var t1 = batch.KeyDeleteAsync(WhitelistKey);
            var t2 = batch.KeyDeleteAsync(BlacklistKey);

            foreach (var rule in rules)
            {
                if (rule.Level == UrlSecurityLevel.Safe)
                {
                    _ = batch.SetAddAsync(WhitelistKey, rule.Host);
                }
                else if (rule.Level == UrlSecurityLevel.Blocked)
                {
                    _ = batch.HashSetAsync(BlacklistKey, rule.Host, rule.Reason ?? "违规阻断");
                }
            }

            batch.Execute();
            await Task.WhenAll(t1, t2);
            Logger.LogInformation($"[UrlSecurity] 刷新 Redis 规则完成，共同步 {rules.Count} 条规则");
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

    private static string ComputeHash(string input)
    {
        using var md5 = MD5.Create();
        var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }
}
