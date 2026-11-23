using IczpNet.AbpTrees;
using IczpNet.Chat.HttpRequests;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Menus;

public interface IMenuManager : ITreeManager<Menu, Guid, MenuInfo>
{
    Task<string> TriggerAsync(Guid id, Guid? sessionUnitId = null, long? senderId = null);

    Task<HttpRequest> SendToRemoteHostAsync(Menu menu, MenuTriggerArgs args = null, string name = null);

    Task CheckMenuAsync(Menu menu);

    Task<bool> IsCheckMenuAsync(Menu menu);

    /// <summary>
    /// 获取已发布菜单快照（支持版本比较与缓存读取）
    /// </summary>
    /// <param name="chatObjectId">公众号/订阅号 ChatObjectId</param>
    /// <returns>发布快照，若从未发布过则返回 null</returns>
    Task<PublishedMenuCacheItem> GetPublishedAsync(long chatObjectId);

    /// <summary>
    /// 发布当前菜单树草稿为线上正式版本并刷新缓存
    /// </summary>
    /// <param name="chatObjectId">公众号/订阅号 ChatObjectId</param>
    /// <returns>最新的发布快照</returns>
    Task<PublishedMenuCacheItem> PublishAsync(long chatObjectId);
}
