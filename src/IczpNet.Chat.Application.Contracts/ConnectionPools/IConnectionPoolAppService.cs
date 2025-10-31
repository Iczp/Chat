﻿using IczpNet.Chat.ConnectionPools.Dtos;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

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
    Task<PagedResultDto<ConnectionPoolDto>> GetListAsync(ConnectionPoolGetListInput input);

    /// <summary>
    ///  
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<ConnectionPoolDto>> GetListByUserAsync(ConnectionPoolGetListInput input);

    /// <summary>
    /// 获取在线人数列表(当前用户)
    /// </summary>
    /// <returns></returns>
    Task<PagedResultDto<ConnectionPoolDto>> GetListByCurrentUserAsync(ConnectionPoolGetListInput input);

    /// <summary>
    ///  获取在线人数列表(聊天对象)
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <returns></returns>
    Task<PagedResultDto<ConnectionPoolDto>> GetListByChatObjectAsync(long chatObjectId);

    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    Task<ConnectionPoolDto> GetAsync(string connectionId);

    /// <summary>
    /// 清空所有连接
    /// </summary>
    /// <param name="host"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task ClearAllAsync(string host, string reason);

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
    /// 获取连接数量(用户)
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> GetCountByUserAsync(Guid userId);

    /// <summary>
    /// 获取连接数量(聊天对象)
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <returns></returns>
    Task<int> GetCountByChatObjectAsync(long chatObjectId);

    /// <summary>
    /// 更新用户连接数量
    /// </summary>
    /// <returns></returns>
    Task<int> UpdateUserAsync(Guid userId);

    /// <summary>
    /// 更新聊天对象连接数量
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <returns></returns>
    Task<int> UpdateChatObjectAsync(long chatObjectId);

    /// <summary>
    /// 强制断开连接
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task AbortAsync(AbortInput input);

}
