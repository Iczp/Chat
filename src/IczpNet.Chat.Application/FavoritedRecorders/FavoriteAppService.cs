using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.FavoritedRecorders.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.FavoritedRecorders;

/// <summary>
/// 消息收藏器
/// </summary>
public class FavoriteAppService : ChatAppService, IFavoriteAppService
{
    protected IFavoritedRecorderManager FavoritedRecorderManager { get; set; }
    protected IRepository<FavoritedRecorder> Repository { get; set; }

    public FavoriteAppService(
        IFavoritedRecorderManager favoritedRecorderManager,
        IRepository<FavoritedRecorder> repository)
    {
        FavoritedRecorderManager = favoritedRecorderManager;
        Repository = repository;
    }

    /// <summary>
    /// 收藏列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<FavoritedRecorderDto>> GetListAsync(FavoritedRecorderGetListInput input)
    {
        var query = (await Repository.GetQueryableAsync())
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
            .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
            .WhereIf(input.MinSize.HasValue, x => x.Size >= input.MinSize)
            .WhereIf(input.MaxSize.HasValue, x => x.Size < input.MaxSize)
            ;
        return await query.ToPagedListAsync<FavoritedRecorder, FavoritedRecorderDto>(AsyncExecuter, ObjectMapper, input);
    }

    /// <summary>
    /// 获取收藏的总大小
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    [HttpGet]
    public Task<long> GetSizeAsync(long ownerId)
    {
        return FavoritedRecorderManager.GetSizeByOwnerIdAsync(ownerId);
    }

    /// <summary>
    /// 获取收藏数量
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    [HttpGet]
    public Task<int> GetCountAsync(long ownerId)
    {
        return FavoritedRecorderManager.GetCountByOwnerIdAsync(ownerId);
    }

    /// <summary>
    /// 添加收藏
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<DateTime> CreateAsync([FromQuery] FavoritedRecorderCreateInput input)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);

        var entity = await FavoritedRecorderManager.CreateIfNotContainsAsync(sessionUnit, input.MessageId, input.DeviceId);

        return entity.CreationTime;
    }

    /// <summary>
    /// 取消收藏
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="messageId">消息Id</param>
    /// <returns></returns>
    [HttpPost]
    public Task DeleteAsync(Guid sessionUnitId, long messageId)
    {
        return FavoritedRecorderManager.DeleteAsync(sessionUnitId, messageId);
    }

   
}
