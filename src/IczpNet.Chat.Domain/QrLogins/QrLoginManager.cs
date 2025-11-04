using IczpNet.AbpCommons;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Clients;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Users;

namespace IczpNet.Chat.QrLogins;

public class QrLoginManager(IOptions<QrLoginOption> options,
    IDistributedEventBus distributedEventBus,
    ICurrentUser currentUser,
    ICurrentClient currentClient) : DomainService, IQrLoginManager
{
    public IDistributedCache<GenerateInfo, string> DistributedCache { get; set; }

    public IDistributedCache<CodeConnectionId, Guid> CodeDistributedCache { get; set; }

    public IDistributedCache<GrantedInfo, Guid> GrantedDistributedCache { get; set; }

    public IOptions<QrLoginOption> Options { get; } = options;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    public ICurrentUser CurrentUser { get; } = currentUser;
    public ICurrentClient CurrentClient { get; } = currentClient;

    protected QrLoginOption Config => Options.Value;

    protected virtual DistributedCacheEntryOptions DistributedCacheEntryOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Config.ExpiredSeconds)
    };

    public async Task<GenerateInfo> GenerateAsync(string connectionId)
    {
        var builder = new StringTemplateBuilder(Config.QrText);

        var res = await DistributedCache.GetAsync(connectionId);

        if ((res != null))
        {
            builder.Parse(res.QrText);

            if (builder.TryGet("code", out var strCode))
            {
                Assert.If(!Guid.TryParse(strCode, out Guid cacheCode), $"无效编码:{cacheCode}", "E101");

                await CodeDistributedCache.RemoveAsync(cacheCode);

                Logger.LogInformation($"删除缓存{cacheCode}");
            }
        }

        var code = Guid.NewGuid();

        builder.Set("code", code);

        var result = new GenerateInfo()
        {
            ConnectionId = connectionId,
            ExpiredTime = Clock.Now.AddSeconds(Config.ExpiredSeconds),
            QrText = builder.ToString()
        };

        await DistributedCache.SetAsync(connectionId, result, DistributedCacheEntryOptions);

        await CodeDistributedCache.SetAsync(code, new CodeConnectionId(connectionId));

        return result;
    }

    protected async Task<GenerateInfo> GetGenerateInfoAsync(string qrText)
    {
        Assert.If(!CurrentUser.Id.HasValue, "请登录后操作", "E100");

        var builder = new StringTemplateBuilder(Config.QrText);

        Assert.If(!builder.TryToValues(qrText, out _), "无法识别", "E101");

        Assert.If(!builder.TryGet("code", out string reqCode), "无法识别[code]", "E102");

        Assert.If(!Guid.TryParse(reqCode, out Guid code), $"无效编码：{reqCode}", "E103");

        var res = await CodeDistributedCache.GetAsync(code);

        Assert.If(res == null, "已经过期", "E104");

        var genarateInfo = await DistributedCache.GetAsync(res.ConnectionId);

        Assert.If(genarateInfo == null, "已经失效", "E105");

        return genarateInfo;
    }

    public async Task<GenerateInfo> ScanAsync(string qrText)
    {
        var genarateInfo = await GetGenerateInfoAsync(qrText);

        Assert.If(genarateInfo.ScanUserId.HasValue && genarateInfo.ScanUserId != CurrentUser.Id, "已经失效：其他用户已经扫码", "E106");

        genarateInfo.ScanUserId = CurrentUser.GetId();

        // 回写
        await DistributedCache.SetAsync(genarateInfo.ConnectionId, genarateInfo, DistributedCacheEntryOptions);

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


    public async Task<GrantedInfo> GrantAsync(string qrText)
    {
        var genarateInfo = await GetGenerateInfoAsync(qrText);

        Assert.If(!genarateInfo.ScanUserId.HasValue, "请先扫码", "E301");

        Assert.If(!CurrentUser.Id.Equals(genarateInfo.ScanUserId), "请用同一用户操作", "E302");

        //生成登录码
        var grantedInfo = new GrantedInfo()
        {
            ConnectionId = genarateInfo.ConnectionId,
            QrLoginCode = Guid.NewGuid(),
            UserId = CurrentUser.Id.Value,
            ExpiredTime = Clock.Now.AddSeconds(Config.ExpiredSeconds),
        };

        await GrantedDistributedCache.SetAsync(grantedInfo.QrLoginCode, grantedInfo);

        //授权成功后删除
        await DistributedCache.RemoveAsync(genarateInfo.ConnectionId);

        return grantedInfo;
    }

    public async Task<RejectInfo> RejectAsync(string qrText)
    {
        var genarateInfo = await GetGenerateInfoAsync(qrText);

        Assert.If(!genarateInfo.ScanUserId.HasValue, "请先扫码", "E401");

        await DistributedCache.RemoveAsync(genarateInfo.ConnectionId);

        return new RejectInfo()
        {
            ConnectionId = genarateInfo.ConnectionId
        };
    }

    public async Task<GrantedInfo> GetGrantedInfoAsync(Guid qrLoginCode)
    {
        var grantedInfo = await GrantedDistributedCache.GetAsync(qrLoginCode);
        return grantedInfo;
    }

    public async Task DeleteGrantedInfoAsync(Guid qrLoginCode)
    {
        await GrantedDistributedCache.RemoveAsync(qrLoginCode);
    }
}
