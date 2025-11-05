using IczpNet.AbpCommons;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Clients;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Users;

namespace IczpNet.Chat.ScanLogins;

public class ScanLoginManager(IOptions<ScanLoginOption> options,
    IDistributedEventBus distributedEventBus,
    IScanLoginChecker scanLoginChecker,
    ICurrentUser currentUser,
    ICurrentClient currentClient) : DomainService, IScanLoginManager
{
    public IDistributedCache<GenerateInfo, string> GenerateDistributedCache { get; set; }

    public IDistributedCache<CodeConnectionId, Guid> CodeDistributedCache { get; set; }

    public IDistributedCache<GrantedInfo, Guid> GrantedDistributedCache { get; set; }

    public IOptions<ScanLoginOption> Options { get; } = options;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    public IScanLoginChecker ScanLoginChecker { get; } = scanLoginChecker;
    public ICurrentUser CurrentUser { get; } = currentUser;
    public ICurrentClient CurrentClient { get; } = currentClient;

    protected ScanLoginOption Config => Options.Value;

    protected string ParamKey => Config.ParamKey;

    protected virtual DistributedCacheEntryOptions DistributedCacheEntryOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Config.ExpiredSeconds)
    };

    public virtual async Task RemoveAsync(string connectionId)
    {
        var res = await GenerateDistributedCache.GetAsync(connectionId);

        if (res == null)
        {
            return;
        }
        var builder = new StringTemplateBuilder(Config.ScanTextTemplate);

        builder.Parse(res.ScanText);

        if (builder.TryGet(ParamKey, out var strCode))
        {
            if (Guid.TryParse(strCode, out Guid cacheCode))
            {
                await CodeDistributedCache.RemoveAsync(cacheCode);
                Logger.LogInformation($"删除缓存{cacheCode}");
            }
            else
            {
                Logger.LogWarning($"无效编码:{cacheCode}");
            }
        }
        await GenerateDistributedCache.RemoveAsync(connectionId);
    }

    public virtual async Task<GenerateInfo> GenerateAsync(string connectionId)
    {
        await RemoveAsync(connectionId);

        var code = Guid.NewGuid();

        var builder = new StringTemplateBuilder(Config.ScanTextTemplate);

        builder.Set(ParamKey, code);

        var result = new GenerateInfo()
        {
            ConnectionId = connectionId,
            ExpiredTime = Clock.Now.AddSeconds(Config.ExpiredSeconds),
            ScanText = builder.ToString()
        };

        await GenerateDistributedCache.SetAsync(connectionId, result, DistributedCacheEntryOptions);

        await CodeDistributedCache.SetAsync(code, new CodeConnectionId(connectionId));

        return result;
    }

    protected virtual async Task<GenerateInfo> GetGenerateInfoAsync(string scanText)
    {

        await ScanLoginChecker.CheckAsync(scanText);

        Assert.If(!CurrentUser.Id.HasValue, "请登录后操作", "E100");

        var builder = new StringTemplateBuilder(Config.ScanTextTemplate);

        Assert.If(!builder.TryToValues(scanText, out _), "无法识别", "E101");

        Assert.If(!builder.TryGet(ParamKey, out string reqCode), "无法识别[code]", "E102");

        Assert.If(!Guid.TryParse(reqCode, out Guid code), $"无效编码：{reqCode}", "E103");

        var res = await CodeDistributedCache.GetAsync(code);

        Assert.If(res == null, "已经过期", "E104");

        var genarateInfo = await GenerateDistributedCache.GetAsync(res.ConnectionId);

        Assert.If(genarateInfo == null, "已经失效", "E105");

        return genarateInfo;
    }

    public virtual async Task<GenerateInfo> ScanAsync(string scanText)
    {
        var genarateInfo = await GetGenerateInfoAsync(scanText);

        Assert.If(genarateInfo.ScanUserId.HasValue && genarateInfo.ScanUserId != CurrentUser.Id, "已经失效：其他用户已经扫码", "E106");

        genarateInfo.ScanUserId = CurrentUser.GetId();

        // 回写
        await GenerateDistributedCache.SetAsync(genarateInfo.ConnectionId, genarateInfo, DistributedCacheEntryOptions);

        await DistributedEventBus.PublishAsync(new LoginActionEto()
        {
            Action = "scan-success",
            Description = "扫码成功",
            UserName = CurrentUser.Name,
            UserId = CurrentUser.Id,
            CliendId = CurrentClient.Id,
        });

        return genarateInfo;
    }


    public virtual async Task<GrantedInfo> GrantAsync(string scanText)
    {
        var genarateInfo = await GetGenerateInfoAsync(scanText);

        Assert.If(!genarateInfo.ScanUserId.HasValue, "请先扫码", "E301");

        Assert.If(!CurrentUser.Id.Equals(genarateInfo.ScanUserId), "请用同一用户操作", "E302");

        //生成登录码
        var grantedInfo = new GrantedInfo()
        {
            ConnectionId = genarateInfo.ConnectionId,
            LoginCode = Guid.NewGuid(),
            UserId = CurrentUser.Id.Value,
            ExpiredTime = Clock.Now.AddSeconds(Config.ExpiredSeconds),
        };

        await GrantedDistributedCache.SetAsync(grantedInfo.LoginCode, grantedInfo);

        //授权成功后删除
        await GenerateDistributedCache.RemoveAsync(genarateInfo.ConnectionId);

        return grantedInfo;
    }

    public virtual async Task<RejectInfo> RejectAsync(string scanText, string reason)
    {
        var genarateInfo = await GetGenerateInfoAsync(scanText);

        Assert.If(!genarateInfo.ScanUserId.HasValue, "请先扫码", "E401");

        await GenerateDistributedCache.RemoveAsync(genarateInfo.ConnectionId);

        return new RejectInfo()
        {
            ConnectionId = genarateInfo.ConnectionId,
            Reason = reason,
        };
    }

    public virtual async Task<CancelInfo> CancelAsync(string connectionId, string reason)
    {
        await RemoveAsync(connectionId);

        return new CancelInfo()
        {
            ConnectionId = connectionId,
            Reason = reason,
        };
    }

    public virtual async Task<GrantedInfo> GetGrantedInfoAsync(Guid loginCode)
    {
        var grantedInfo = await GrantedDistributedCache.GetAsync(loginCode);
        return grantedInfo;
    }

    public virtual async Task DeleteGrantedInfoAsync(Guid loginCode)
    {
        await GrantedDistributedCache.RemoveAsync(loginCode);
    }
}
