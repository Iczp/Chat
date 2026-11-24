using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.FavoritedRecorders.Dtos;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
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
        var normalizedExts = input.Extensions?
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().TrimStart('.').ToLower())
            .Distinct()
            .ToList();

        var query = (await Repository.GetQueryableAsync())
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
            .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
            .WhereIf(input.SessionUnitId.HasValue, x => x.SessionUnitId == input.SessionUnitId)
            .WhereIf(input.SessionId.HasValue, x => x.Message.SessionId == input.SessionId)
            .WhereIf(input.SenderId.HasValue, x => x.Message.SenderId == input.SenderId)
            .WhereIf(input.MinSize.HasValue, x => x.Size >= input.MinSize)
            .WhereIf(input.MaxSize.HasValue, x => x.Size < input.MaxSize)
            .WhereIf(input.MessageType.HasValue, x => x.MessageType == input.MessageType)
            .WhereIf(input.MessageTypes != null && input.MessageTypes.Count > 0, x => input.MessageTypes.Contains(x.MessageType))
            .WhereIf(input.StartTime.HasValue, x => x.CreationTime >= input.StartTime)
            .WhereIf(input.EndTime.HasValue, x => x.CreationTime < input.EndTime)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x =>
                x.Message.TextContentList.Any(t => t.Text.Contains(input.Keyword)) ||
                x.Message.FileContentList.Any(f => f.FileName.Contains(input.Keyword)) ||
                x.Message.ImageContentList.Any(i => i.FileName.Contains(input.Keyword)) ||
                x.Message.VideoContentList.Any(v => v.FileName.Contains(input.Keyword)) ||
                x.Message.SoundContentList.Any(s => s.FileName.Contains(input.Keyword)))
            .WhereIf(normalizedExts != null && normalizedExts.Count > 0, x =>
                x.Message.FileContentList.Any(f => normalizedExts.Contains(f.Suffix.ToLower())) ||
                x.Message.ImageContentList.Any(i => normalizedExts.Contains(i.Suffix.ToLower())) ||
                x.Message.VideoContentList.Any(v => normalizedExts.Contains(v.Suffix.ToLower())) ||
                x.Message.SoundContentList.Any(s => normalizedExts.Contains(s.Suffix.ToLower())))
            ;

        if (string.IsNullOrWhiteSpace(input.Sorting))
        {
            input.Sorting = $"{nameof(FavoritedRecorder.CreationTime)} desc";
        }

        return await GetPagedListAsync<FavoritedRecorder, FavoritedRecorderDto>(query, input);
    }

    /// <summary>
    /// 获取收藏的总大小
    /// </summary>
    /// <param name="ownerId">聊天对象Id</param>
    /// <returns></returns>
    [HttpGet]
    public Task<long> GetSizeAsync([Required] long ownerId)
    {
        return FavoritedRecorderManager.GetSizeByOwnerIdAsync(ownerId);
    }

    /// <summary>
    /// 获取收藏数量
    /// </summary>
    /// <param name="ownerId">聊天对象Id</param>
    /// <returns></returns>
    [HttpGet]
    public Task<int> GetCountAsync([Required] long ownerId)
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
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public Task DeleteAsync([FromQuery] FavoritedRecorderDeleteInput input)
    {
        return FavoritedRecorderManager.DeleteAsync(input.SessionUnitId, input.MessageId);
    }

    /// <summary>
    /// 添加/取消收藏
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpPost]
    public async Task<bool> SetAsync([FromQuery] FavoritedRecorderInput input)
    {
        if (input.IsFavorite)
        {
            var sessionUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);
            await FavoritedRecorderManager.CreateIfNotContainsAsync(sessionUnit, input.MessageId, input.DeviceId);
        }
        else
        {
            await FavoritedRecorderManager.DeleteAsync(input.SessionUnitId, input.MessageId);
        }
        return input.IsFavorite;
    }
}
