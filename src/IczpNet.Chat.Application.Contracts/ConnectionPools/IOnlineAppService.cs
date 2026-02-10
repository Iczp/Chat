using IczpNet.AbpCommons.Dtos;
using IczpNet.Chat.ConnectionPools.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ConnectionPools;

public interface IOnlineAppService
{
    /// <summary>
    /// 获取所有主机在线人数
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<OnlineHostDto>> GetHostsAsync(ConnectionHostGetListInput input);

    /// <summary>
    /// 获取在线人数
    /// </summary>
    /// <returns></returns>
    Task<long> GetTotalCountAsync(string host);

    /// <summary>
    /// 获取在线人数列表
    /// </summary>
    /// <returns></returns>
    Task<PagedResultDto<ConnectionPoolDto>> GetListAsync(ConnectionPoolGetListInput input);

    /// <summary>
    ///  获取用户连接
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<ConnectionPoolDto>> GetListByUserAsync(Guid userId, GetListInput input);

    /// <summary>
    /// 获取在线人数列表(当前用户)
    /// </summary>
    /// <returns></returns>
    Task<PagedResultDto<ConnectionPoolDto>> GetListByCurrentUserAsync(GetListInput input);

    /// <summary>
    ///  获取在线人数列表(聊天对象)
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<ConnectionPoolDto>> GetListByOwnerAsync(long ownerId, GetListInput input);

    /// <summary>
    /// 获取连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    Task<ConnectionPoolDto> GetAsync(string connectionId);

    /// <summary>
    /// 获取连接(多个)
    /// </summary>
    /// <param name="connectionIds"></param>
    /// <returns></returns>
    Task<Dictionary<string, ConnectionPoolDto>> GetManyAsync(List<string> connectionIds);

    /// <summary>
    /// 清空所有连接
    /// </summary>
    /// <param name="hosts"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task<Dictionary<string, long>> ClearAllAsync(List<string> hosts, string reason);

    /// <summary>
    /// 移除连接
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    Task RemoveAsync(string connectionId);

    /// <summary>
    /// 获取连接数量(用户)
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<long> GetCountByUserAsync(Guid userId);

    /// <summary>
    /// 获取连接数量(聊天对象)
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<long> GetCountByOwnerAsync(long ownerId);

    /// <summary>
    /// 获取连接数量(会话)
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<long> GetCountBySessionAsync(Guid sessionId);

    /// <summary>
    /// 获取最近在线列表
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<LastOnline>> GetLastOnlineAsync(long ownerId, LastOnlineGetListInput input);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<long> GetOnlineFriendsCountAsync(long ownerId);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<OnlineFriendDto>> GetOnlineFriendsAsync(OnlineFriendsGetListInput input);

    /// <summary>
    /// 强制断开连接
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task AbortAsync(AbortInput input);

}
