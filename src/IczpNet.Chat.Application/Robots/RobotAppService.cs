using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Robots.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Robots;

/// <summary>
/// 机器人
/// </summary>
public class RobotAppService : ChatAppService, IRobotAppService
{
    protected IChatObjectRepository Repository { get; }
    protected IChatObjectCategoryManager ChatObjectCategoryManager { get; }
    public RobotAppService(IChatObjectRepository repository, IChatObjectCategoryManager chatObjectCategoryManager)
    {
        Repository = repository;
        ChatObjectCategoryManager = chatObjectCategoryManager;
    }


    protected virtual Task<IQueryable<ChatObject>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        return Repository.GetQueryableAsync();
    }

    /// <summary>
    /// 获取机器人列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public virtual async Task<PagedResultDto<RobotDto>> GetListAsync(RobotGetListInput input)
    {
        //Category
        IQueryable<Guid> categoryIdQuery = null;

        if (input.IsImportChildCategory == true && input.CategoryIdList.IsAny())
        {
            categoryIdQuery = (await ChatObjectCategoryManager.QueryCurrentAndAllChildsAsync(input.CategoryIdList)).Select(x => x.Id);
        }
        var query = (await CreateFilteredQueryAsync(input))
              .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled)
              //CategoryId
              .WhereIf(input.IsImportChildCategory == false && input.CategoryIdList.IsAny(), x => x.ChatObjectCategoryUnitList.Any(d => input.CategoryIdList.Contains(d.CategoryId)))
              .WhereIf(input.IsImportChildCategory == true && input.CategoryIdList.IsAny(), x => x.ChatObjectCategoryUnitList.Any(d => categoryIdQuery.Contains(d.CategoryId)))
              .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword) || x.Code.Contains(input.Keyword) || x.NameSpellingAbbreviation.Contains(input.Keyword))
              ;

        return await query.ToPagedListAsync<ChatObject, RobotDto>(AsyncExecuter, ObjectMapper, input, x => x.OrderBy(d => d.Name));

    }
}
