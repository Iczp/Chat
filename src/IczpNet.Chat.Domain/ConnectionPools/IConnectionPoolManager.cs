using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.ConnectionPools;

/// <summary>
/// 连接池管理器
/// </summary>
public interface IConnectionPoolManager: IConnectionPoolManagerBase<ConnectionPoolCacheItem>
{
    /// <summary>
    /// 获取用户连接
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<List<string>> GetConnectionIdsByUserAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// 更新连接数量
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<int> UpdateIndexByUserAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// 更新连接数量
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<int> UpdateIndexByChatObjectAsync(long chatObjectId, CancellationToken token = default);

    /// <summary>
    /// 获取聊天对象连接
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<IEnumerable<ConnectionPoolCacheItem>> GetListByChatObjectAsync(long chatObjectId, CancellationToken token = default);

    /// <summary>
    /// 获取聊天对象连接
    /// </summary>
    /// <param name="chatObjectIdList"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<IEnumerable<ConnectionPoolCacheItem>> GetListByChatObjectAsync(IEnumerable<long> chatObjectIdList, CancellationToken token = default);

    /// <summary>
    /// 获取 用户 连接
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<IEnumerable<ConnectionPoolCacheItem>> GetListByUserAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// 获取 用户 连接
    /// </summary>
    /// <param name="userIdList"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<IEnumerable<ConnectionPoolCacheItem>> GetListByUserAsync(IEnumerable<Guid> userIdList, CancellationToken token = default);

    /// <summary>
    /// 是否在线（用户）
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> IsOnlineAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// 是否在线（聊天对象）
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> IsOnlineAsync(long chatObjectId, CancellationToken token = default);

    /// <summary>
    /// 获取设备类型 聊天对象
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<List<string>> GetDeviceTypesAsync(long chatObjectId, CancellationToken token = default);

    /// <summary>
    /// 获取设备类型 用户
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<List<string>> GetDeviceTypesAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// 获取连接数量(用户)
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<int> GetCountByUserAsync(Guid userId, CancellationToken token = default);

    /// <summary>
    /// 获取连接数量(聊天对象)
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<int> GetCountByChatObjectAsync(long chatObjectId, CancellationToken token = default);
}
