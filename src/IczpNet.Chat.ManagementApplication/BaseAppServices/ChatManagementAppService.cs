﻿using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Localization;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.BaseAppServices;

public abstract class ChatManagementAppService : ApplicationService
{
    protected ICurrentChatObject CurrentChatObject => LazyServiceProvider.LazyGetRequiredService<ICurrentChatObject>();
    protected ChatManagementAppService()
    {
        LocalizationResource = typeof(ChatResource);
        ObjectMapperContext = typeof(ChatManagementApplicationModule);
    }

    protected virtual async Task<PagedResultDto<TOuputDto>> GetPagedListAsync<TEntity, TOuputDto>(
        IQueryable<TEntity> query,
        int maxResultCount = 10,
        int skipCount = 0, string sorting = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> action = null)
    {
        var totalCount = await AsyncExecuter.CountAsync(query);

        if (!sorting.IsNullOrWhiteSpace())
        {
            query = query.OrderBy(sorting);
        }
        else if (action != null)
        {
            query = action.Invoke(query);
        }

        query = query.PageBy(skipCount, maxResultCount);

        var entities = await AsyncExecuter.ToListAsync(query);

        var items = ObjectMapper.Map<List<TEntity>, List<TOuputDto>>(entities);

        return new PagedResultDto<TOuputDto>(totalCount, items);
    }

    protected virtual Task<PagedResultDto<TOuputDto>> GetPagedListAsync<TEntity, TOuputDto>(
        IQueryable<TEntity> query,
        PagedAndSortedResultRequestDto input,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> action = null)
    {
        return GetPagedListAsync<TEntity, TOuputDto>(query, input.MaxResultCount, input.SkipCount, input.Sorting, action);
    }

    protected virtual async Task CheckPolicyAsync(string policyName, ChatObject owner)
    {
        if (string.IsNullOrEmpty(policyName))
        {
            return;
        }

        await AuthorizationService.CheckAsync(owner, policyName);
    }

    protected virtual async Task CheckPolicyAsync(string policyName, SessionUnit sessionUnit)
    {
        if (string.IsNullOrEmpty(policyName))
        {
            return;
        }

        await AuthorizationService.CheckAsync(sessionUnit, policyName);
    }
}
