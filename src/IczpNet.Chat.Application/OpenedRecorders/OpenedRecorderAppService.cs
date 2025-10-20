using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Devices;
using IczpNet.Chat.OpenedRecorders.Dtos;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.OpenedRecorders;

/// <summary>
/// 消息[打开]记录器
/// </summary>
public class OpenedRecorderAppService(
    IOpenedRecorderManager openedRecorderManager,
    IRepository<OpenedRecorder> repository,
    IDeviceResolver deviceResolver) : ChatAppService, IOpenedRecorderAppService
{
    protected virtual string SetReadedPolicyName { get; set; }

    protected IRepository<OpenedRecorder> Repository { get; } = repository;

    protected IOpenedRecorderManager OpenedRecorderManager { get; } = openedRecorderManager;

    protected IDeviceResolver DeviceIdResolver { get; } = deviceResolver;

    /// <summary>
    /// 获取消息【已打开】的数量
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    [HttpGet]
    public Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList)
    {
        return OpenedRecorderManager.GetCountsAsync(messageIdList);
    }

    /// <summary>
    /// 获取消息【已打开】的聊天对象
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDestinationDto>> GetListByMessageIdAsync(GetListByMessageIdInput input)
    {
        var query = input.IsReaded
            ? await OpenedRecorderManager.QueryRecordedAsync(input.MessageId)
            : await OpenedRecorderManager.QueryUnrecordedAsync(input.MessageId);

        query = query.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordOwnerSessionUnitSpecification(input.Keyword, await ChatObjectManager.QueryByKeywordAsync(input.Keyword)));

        return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input);
    }

    /// <summary>
    /// 设置为【已打开】
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<OpenedRecorderDto> SetOpenedAsync([FromQuery] OpenedRecorderInput input)
    {
        var deviceId = await OpenedRecorderManager.CheckDeviceIdAsync(input.DeviceId);

        await CheckPolicyAsync(SetReadedPolicyName);

        var sessionUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);

        var entity = await OpenedRecorderManager.CreateIfNotContainsAsync(sessionUnit, input.MessageId, deviceId);

        return await MapToDtoAsync(entity);
    }

    protected virtual Task<OpenedRecorderDto> MapToDtoAsync(OpenedRecorder entity)
    {
        return Task.FromResult(MapToDto(entity));
    }

    protected virtual OpenedRecorderDto MapToDto(OpenedRecorder entity)
    {
        return ObjectMapper.Map<OpenedRecorder, OpenedRecorderDto>(entity);
    }
}
