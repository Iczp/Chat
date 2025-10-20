using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.DeletedRecorders.Dtos;
using IczpNet.Chat.Devices;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.DeletedRecorders;

/// <summary>
/// 消息[删除]记录器
/// </summary>
public class DeletedRecorderAppService(
    IDeletedRecorderManager openedRecorderManager,
    IRepository<DeletedRecorder> repository,
    IDeviceResolver deviceResolver) : ChatAppService, IDeletedRecorderAppService
{
    protected virtual string SetReadedPolicyName { get; set; }

    protected IRepository<DeletedRecorder> Repository { get; } = repository;

    protected IDeletedRecorderManager DeletedRecorderManager { get; } = openedRecorderManager;

    protected IDeviceResolver DeviceResolver { get; } = deviceResolver;

    /// <summary>
    /// 获取消息【已删除】的数量
    /// </summary>
    /// <param name="messageIdList"></param>
    /// <returns></returns>
    [HttpGet]
    public Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList)
    {
        return DeletedRecorderManager.GetCountsAsync(messageIdList);
    }

    /// <summary>
    /// 获取消息【已删除】的聊天对象
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDestinationDto>> GetListByMessageIdAsync(GetListByMessageIdInput input)
    {
        var query = input.IsDeleted
            ? await DeletedRecorderManager.QueryRecordedAsync(input.MessageId)
            : await DeletedRecorderManager.QueryUnrecordedAsync(input.MessageId);

        query = query.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordOwnerSessionUnitSpecification(input.Keyword, await ChatObjectManager.QueryByKeywordAsync(input.Keyword)));

        return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input);
    }

    /// <summary>
    /// 设置为【已删除】
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<DeletedRecorderDto> SetDeletedAsync([FromQuery] DeletedRecorderInput input)
    {
        var deviceId = await DeletedRecorderManager.CheckDeviceIdAsync(input.DeviceId);

        await CheckPolicyAsync(SetReadedPolicyName);

        var sessionUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);

        var entity = await DeletedRecorderManager.CreateIfNotContainsAsync(sessionUnit, input.MessageId, deviceId);

        return await MapToDtoAsync(entity);
    }

    protected virtual Task<DeletedRecorderDto> MapToDtoAsync(DeletedRecorder entity)
    {
        return Task.FromResult(MapToDto(entity));
    }

    protected virtual DeletedRecorderDto MapToDto(DeletedRecorder entity)
    {
        return ObjectMapper.Map<DeletedRecorder, DeletedRecorderDto>(entity);
    }
}
