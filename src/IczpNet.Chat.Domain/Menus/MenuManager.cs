using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using IczpNet.BizCrypts;
using IczpNet.Chat.Developers;
using IczpNet.Chat.HttpRequests;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Json;

namespace IczpNet.Chat.Menus;

public class MenuManager : TreeManager<Menu, Guid, MenuInfo>, IMenuManager
{
    protected IBackgroundJobManager BackgroundJobManager { get; }
    protected IHttpRequestManager HttpRequestManager { get; }
    protected IJsonSerializer JsonSerializer { get; }
    protected IDistributedCache<PublishedMenuCacheItem, string> PublishedMenuCache { get; }

    public MenuManager(
        IRepository<Menu, Guid> repository,
        IBackgroundJobManager backgroundJobManager,
        IHttpRequestManager httpRequestManager,
        IJsonSerializer jsonSerializer,
        IDistributedCache<PublishedMenuCacheItem, string> publishedMenuCache) : base(repository)
    {
        BackgroundJobManager = backgroundJobManager;
        HttpRequestManager = httpRequestManager;
        JsonSerializer = jsonSerializer;
        PublishedMenuCache = publishedMenuCache;
    }

    protected override async Task CheckExistsByCreateAsync(Menu inputEntity)
    {
        Assert.If(await Repository.AnyAsync(x => x.Name == inputEntity.Name && x.OwnerId == inputEntity.OwnerId), $"Already exists Name:{inputEntity.Name}");
    }

    protected override async Task CheckExistsByUpdateAsync(Menu inputEntity)
    {
        Assert.If(await Repository.AnyAsync((x) => x.Name == inputEntity.Name && x.OwnerId == inputEntity.OwnerId && !x.Id.Equals(inputEntity.Id)), $" Name[{inputEntity.Name}] already such");
    }

    /// <summary>
    /// ParseUrl
    /// </summary>
    /// <param name="url"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    protected virtual string ParseUrl(string url, IDictionary<string, string> parameters)
    {
        var _uri = new Uri(url);

        var _params = parameters.Select(x => $"{x.Key}={HttpUtility.UrlEncode(x.Value)}").JoinAsString("&");

        var _query = new StringBuilder(_uri.Query);

        _query.Append(url.IndexOf('?') != -1 ? "&" : "?");

        _query.Append(_params);

        var retUrl = new string[] { _uri.Scheme, "://", _uri.Authority, _uri.AbsolutePath, _query.ToString(), _uri.Fragment };

        return retUrl.JoinAsString("");
    }

    public virtual async Task<string> TriggerAsync(Guid id, Guid? sessionUnitId = null, long? senderId = null)
    {
        var jobId = await BackgroundJobManager.EnqueueAsync(new MenuTriggerArgs()
        {
            MenuId = id,
            SessionUnitId = sessionUnitId,
            SenderId = senderId
        });

        Logger.LogInformation($"Trigger:MenuId={id},SessionUnitId={sessionUnitId},SenderId={senderId},jobId={jobId}");

        return jobId;
    }

    public virtual async Task CheckMenuAsync(Menu menu)
    {
        var owner = Assert.NotNull(menu.Owner, $"Owner is null");

        Assert.If(!owner.IsEnabled, $"IsEnabled:{owner.IsEnabled}");

        Assert.If(!owner.IsDeveloper, $"IsDeveloper:{owner.IsDeveloper}");

        var developer = Assert.NotNull(owner.Developer, $"Developer is null");

        Assert.If(!IsUrl(developer.PostUrl), $"Fail Url:{developer.PostUrl}", nameof(developer.PostUrl));

        await Task.Yield();
    }

    public virtual async Task<bool> IsCheckMenuAsync(Menu menu)
    {
        await Task.Yield();

        var owner = menu.Owner;

        return owner != null && owner.IsEnabled && owner.IsDeveloper && IsUrl(owner.Developer?.PostUrl);
    }

    private static bool IsUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var httpSchemes = new string[] { Uri.UriSchemeHttp, Uri.UriSchemeHttps };

        return Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri uri) && httpSchemes.Contains(uri.Scheme);
    }

    public virtual async Task<HttpRequest> SendToRemoteHostAsync(Menu menu, MenuTriggerArgs args = null, string name = null)
    {
        var developer = menu.Owner?.Developer;
        if (developer == null || string.IsNullOrWhiteSpace(developer.PostUrl))
        {
            throw new InvalidOperationException("Developer or PostUrl is not configured.");
        }

        var payload = JsonSerializer.Serialize(new
        {
            ToUserName = menu.Owner?.Code ?? menu.OwnerId.ToString(),
            FromUserName = args?.SenderId?.ToString() ?? "",
            CreateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            MsgType = "event",
            Event = menu.Event.ToString(),
            EventKey = menu.Code ?? menu.Name,
            MenuId = menu.Id.ToString(),
            SessionUnitId = args?.SessionUnitId?.ToString()
        });

        var bizCrypt = new BizCrypt(developer.Token, developer.EncodingAesKey, developer.OwnerId.ToString());
        var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var nonce = BizCrypt.CreateRandCode(10, "123456789");
        var echo = bizCrypt.Encrypt(payload);
        var signature = BizCrypt.GenerateSignature(developer.Token, timeStamp, nonce, echo);

        var remoteUrl = ParseUrl(developer.PostUrl, new Dictionary<string, string>
        {
            { "signature", signature },
            { "timeStamp", timeStamp },
            { "nonce", nonce }
        });

        var jsonBody = JsonSerializer.Serialize(new { encrypt = echo });
        var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var responseResult = await HttpRequestManager.RequestAsync(HttpMethod.Post, remoteUrl, httpContent, name: name);
        Logger.LogInformation($"SendToRemoteHost ReqId={responseResult.Id},[POST,{responseResult.StatusCode}],url={responseResult.Url}");
        return responseResult;
    }

    private static string GetCacheKey(long chatObjectId) => $"Chat:Menu:Published:{chatObjectId}";

    public virtual async Task<PublishedMenuCacheItem> GetPublishedAsync(long chatObjectId)
    {
        var cacheKey = GetCacheKey(chatObjectId);
        var cached = await PublishedMenuCache.GetAsync(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        // 冷启动从数据库加载当前配置并自动发布快照（保证已有公众号菜单不丢失）
        return await PublishAsync(chatObjectId);
    }

    public virtual async Task<PublishedMenuCacheItem> PublishAsync(long chatObjectId)
    {
        var entities = await Repository.GetListAsync(x => x.OwnerId == chatObjectId);
        var ordered = entities.OrderByDescending(x => x.Sorting).ToList();

        // 构建已发布菜单树快照
        var itemMap = ordered.Select(x => new PublishedMenuItem
        {
            Id = x.Id,
            ParentId = x.ParentId,
            Name = x.Name,
            OwnerId = x.OwnerId,
            Event = x.Event.ToString(),
            Code = x.Code,
            Args = x.Args,
            Sorting = x.Sorting,
            Children = []
        }).ToDictionary(x => x.Id);

        var rootItems = new List<PublishedMenuItem>();
        foreach (var item in itemMap.Values)
        {
            if (item.ParentId.HasValue && itemMap.TryGetValue(item.ParentId.Value, out var parent))
            {
                parent.Children.Add(item);
            }
            else
            {
                rootItems.Add(item);
            }
        }

        var now = Clock.Now;
        var version = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        var snapshot = new PublishedMenuCacheItem
        {
            ChatObjectId = chatObjectId,
            Version = version,
            PublishTime = now,
            Items = rootItems
        };

        var cacheKey = GetCacheKey(chatObjectId);
        await PublishedMenuCache.SetAsync(cacheKey, snapshot);
        Logger.LogInformation($"[MenuManager] Published menu for ChatObjectId={chatObjectId}, Version={version}, RootCount={rootItems.Count}");
        return snapshot;
    }
}
