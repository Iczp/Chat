using IczpNet.Chat.ConnectionPools.Dtos;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.ConnectionPools;

public interface IConnectionPoolAppService
{

    /// <summary>
    /// 获取在线人数
    /// </summary>
    /// <returns></returns>
    Task<int> GetTotalCountAsync(string host);

    /// <summary>
    /// 获取在线人数列表
    /// </summary>
    /// <returns></returns>
    Task<PagedResultDto<ConnectionPoolCacheItem>> GetListAsync(ConnectionPoolGetListInput input);
    /// <summary>
    /// 获取在线人数列表(聊天对象)
    /// </summary>
    /// <param name="chatObjectIdList"></param>
    /// <returns></returns>
    Task<PagedResultDto<ConnectionPoolCacheItem>> GetListByChatObjectAsync(List<long> chatObjectIdList);

    /// <summary>
    /// 获取在线人数列表(当前用户)
    /// </summary>
    /// <returns></returns>
    Task<PagedResultDto<ConnectionPoolCacheItem>> GetListByCurrentUserAsync(ConnectionPoolGetListInput input);
    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    Task<ConnectionPoolCacheItem> GetAsync(string connectionId);

    /// <summary>
    /// 清空所有连接
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    Task ClearAllAsync(string host);

    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    Task RemoveAsync(string connectionId);

    /// <summary>
    /// 更新连接数量
    /// </summary>
    /// <returns></returns>
    Task<int> UpdateConnectionIdsAsync();


    /// <summary>
    /// 获取用户连接
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<string>> GetConnectionIdsByUserIdAsync(Guid userId);

    /// <summary>
    /// 获取用户连接
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> GetCountByUserIdAsync(Guid userId);

    /// <summary>
    /// 更新用户连接数量
    /// </summary>
    /// <returns></returns>
    Task<int> UpdateUserConnectionIdsAsync(Guid userId);

    /// <summary>
    /// 强制断开连接
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task AbortAsync(AbortInput input);

}
