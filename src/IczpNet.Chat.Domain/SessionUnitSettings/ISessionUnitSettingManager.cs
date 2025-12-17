using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionUnitSettings;

public interface ISessionUnitSettingManager
{
    /// <summary>
    /// 分片搜索 Rename 
    /// </summary>
    /// <param name="keyword"></param>
    /// <param name="batchSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>List&lt;Guid&gt; SessionUnitId</returns>
    IAsyncEnumerable<Guid> BatchSearchAsync(
        string keyword,
        int batchSize = 1000,
        CancellationToken cancellationToken = default);
    /// <summary>
    /// 搜索 Rename 
    /// </summary>
    /// <param name="keyword"></param>
    /// <returns>List&lt;Guid&gt; SessionUnitId</returns>
    Task<List<Guid>> SearchRenameIdsAsync(string keyword);
    /// <summary>
    /// 设置成员名称
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="memberName"></param>
    /// <returns></returns>
    Task<SessionUnitSetting> SetMemberNameAsync(Guid sessionUnitId, string memberName);

    /// <summary>
    /// 设置重命名
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="rename"></param>
    /// <returns></returns>
    Task<SessionUnitSetting> SetRenameAsync(Guid sessionUnitId, string rename);

    ///// <summary>
    ///// 设置置顶
    ///// </summary>
    ///// <param name="sessionUnitId"></param>
    ///// <param name="isTopping"></param>
    ///// <returns></returns>
    //Task<SessionUnitSetting> SetToppingAsync(Guid sessionUnitId, bool isTopping);

    /// <summary>
    /// 设置是否为沉浸式
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    Task<SessionUnitSetting> SetImmersedAsync(Guid sessionUnitId, bool isImmersed);

    /// <summary>
    /// 设置是否为联系人
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="isContacts"></param>
    /// <returns></returns>
    Task<SessionUnitSetting> SetIsContactsAsync(Guid sessionUnitId, bool isContacts);

    /// <summary>
    /// 设置是否显示成员名称
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="isShowMemberName"></param>
    /// <returns></returns>
    Task<SessionUnitSetting> SetIsShowMemberNameAsync(Guid sessionUnitId, bool isShowMemberName);

    /// <summary>
    /// 移除会话
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    Task<SessionUnitSetting> RemoveAsync(Guid sessionUnitId);

    /// <summary>
    /// 删除会话
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    Task<SessionUnitSetting> KillAsync(Guid sessionUnitId);

    /// <summary>
    /// 清空消息
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    Task<SessionUnitSetting> ClearMessageAsync(Guid sessionUnitId);

    /// <summary>
    /// 删除消息
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    Task<SessionUnitSetting> DeleteMessageAsync(Guid sessionUnitId, long messageId);

    /// <summary>
    /// 禁言过期时间，为空则不禁言
    /// </summary>
    /// <param name="muterSessionUnitId"></param>
    /// <param name="muteExpireTime"></param>
    /// <param name="setterSessionUnit"></param>
    /// <param name="isSendMessage"></param>
    /// <returns></returns>
    Task<DateTime?> SetMuteExpireTimeAsync(Guid muterSessionUnitId, DateTime? muteExpireTime, SessionUnit setterSessionUnit, bool isSendMessage);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitIdList"></param>
    /// <returns></returns>
    Task<KeyValuePair<Guid, SessionUnitSettingCacheItem>[]> GetManyCacheAsync(List<Guid> unitIdList);
}
