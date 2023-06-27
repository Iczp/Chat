using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ReadedRecorders.Dtos;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ReadedRecorders;

/// <summary>
/// 消息【已读】记录器
/// </summary>
public class ReadedRecorderAppService : ChatAppService, IReadedRecorderAppService
{
    protected string SetReadedManyPolicyName { get; set; }
    protected IReadedRecorderManager ReadedRecorderManager { get; }
    public ReadedRecorderAppService(
        IReadedRecorderManager readedRecorderManager)
    {
        ReadedRecorderManager = readedRecorderManager;
    }

    /// <summary>
    /// 获取【已读】的数量
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    [HttpGet]
    public Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList)
    {
        return ReadedRecorderManager.GetCountsAsync(messageIdList);
    }

    /// <summary>
    /// 获取消息已读的聊天对象
    /// </summary>
    /// <param name="messageId">消息Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDestinationDto>> GetListByMessageIdAsync(long messageId, GetListByMessageIdInput input)
    {
        var query = input.IsReaded
            ? await ReadedRecorderManager.QueryRecordedAsync(messageId)
            : await ReadedRecorderManager.QueryUnrecordedAsync(messageId);

        query = query.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordOwnerSessionUnitSpecification(input.Keyword, await ChatObjectManager.QueryByKeywordAsync(input.Keyword)));

        return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input);
    }

    /// <summary>
    /// 设置为【忌讳】
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<int> SetReadedManyAsync([FromQuery] SetReadedManyInput input)
    {
        await CheckPolicyAsync(SetReadedManyPolicyName);

        var sessionUnit = await SessionUnitManager.GetAsync(input.SessunitUnitId);

        var entities = await ReadedRecorderManager.CreateManyAsync(sessionUnit, input.MessageIdList, input.DeviceId);

        return entities.Count;
    }

    /// <summary>
    /// 消息全部设置为已读
    /// </summary>
    /// <param name="messageId">消息Id</param>
    /// <returns></returns>
    [HttpPost]
    public Task<int> SetAllAsync(long messageId)
    {
        return ReadedRecorderManager.CreateAllAsync(messageId);
    }
}
