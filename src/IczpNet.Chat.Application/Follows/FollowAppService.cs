using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Follows.Dtos;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Follows;

/// <summary>
/// 关注
/// </summary>
public class FollowAppService(
    IFollowManager followManager,
    ISessionUnitRepository sessionUnitRepository,
    IRepository<Follow> repository) : ChatAppService, IFollowAppService
{
    protected IFollowManager FollowManager { get; set; } = followManager;
    protected ISessionUnitRepository SessionUnitRepository { get; set; } = sessionUnitRepository;
    protected IRepository<Follow> Repository { get; set; } = repository;

    /// <summary>
    /// 我关注的
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDestinationDto>> GetListFollowingAsync(FollowingGetListInput input)
    {
        var ownerSessionUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);

        var ownerSessionUnitIdList = (await Repository.GetQueryableAsync())
            .Where(x => x.OwnerSessionUnitId == ownerSessionUnit.Id)
            .Select(x => x.DestinationSessionUnitId)
            ;

        var query = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => ownerSessionUnitIdList.Contains(x.Id))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordOwnerSessionUnitSpecification(input.Keyword, await ChatObjectManager.QueryByKeywordAsync(input.Keyword)))
            ;

        return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input);
    }

    /// <summary>
    /// 关注我的
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDestinationDto>> GetListFollowerAsync(FollowerGetListInput input)
    {
        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.DestinationSessionUnitId == input.SessionUnitId)
            .Select(x => x.OwnerSessionUnit)
            //.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordDestinationSessionUnitSpecification(input.Keyword).ToExpression())
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordDestinationSessionUnitSpecification(input.Keyword, await ChatObjectManager.QueryByKeywordAsync(input.Keyword)))
            ;

        return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input);
    }

    /// <summary>
    /// 添加关注
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<bool> CreateAsync([FromQuery] FollowCreateInput input)
    {
        var owner = await SessionUnitManager.GetAsync(input.SessionUnitId);

        //check owner

        return await FollowManager.CreateAsync(owner, input.IdList);
    }

    /// <summary>
    /// 取消关注
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task DeleteAsync([FromQuery] FollowDeleteInput input)
    {
        var owner = await SessionUnitManager.GetAsync(input.SessionUnitId);
        //check owner
        await FollowManager.DeleteAsync(input.SessionUnitId, input.IdList);
    }


}
