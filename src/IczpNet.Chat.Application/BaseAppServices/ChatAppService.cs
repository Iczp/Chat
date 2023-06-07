using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Localization;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Auditing;

namespace IczpNet.Chat.BaseAppServices;

[ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
[Authorize]
public abstract class ChatAppService : ApplicationService
{
    protected virtual string CreatePolicyName { get; set; }
    protected virtual string UpdatePolicyName { get; set; }
    protected virtual string DeletePolicyName { get; set; }
    protected virtual string GetPolicyName { get; set; }
    protected virtual string GetListPolicyName { get; set; }

    protected ICurrentChatObject CurrentChatObject => LazyServiceProvider.LazyGetRequiredService<ICurrentChatObject>();
    protected ChatAppService()
    {
        LocalizationResource = typeof(ChatResource);
        ObjectMapperContext = typeof(ChatApplicationModule);
    }

    protected virtual async Task<PagedResultDto<TOuputDto>> GetPagedListAsync<TEntity, TOuputDto>(
        IQueryable<TEntity> query,
        int maxResultCount = 10,
        int skipCount = 0, string sorting = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryableAction = null, Func<List<TEntity>, Task<List<TEntity>>> entityAction = null)
    {
        var totalCount = await AsyncExecuter.CountAsync(query);

        if (!sorting.IsNullOrWhiteSpace())
        {
            query = query.OrderBy(sorting);
        }
        else if (queryableAction != null)
        {
            query = queryableAction.Invoke(query);
        }

        query = query.PageBy(skipCount, maxResultCount);

        var entities = await AsyncExecuter.ToListAsync(query);

        if (entityAction != null)
        {
            entities = await entityAction?.Invoke(entities);
        }

        var items = ObjectMapper.Map<List<TEntity>, List<TOuputDto>>(entities);

        return new PagedResultDto<TOuputDto>(totalCount, items);
    }

    protected virtual Task<PagedResultDto<TOuputDto>> GetPagedListAsync<TEntity, TOuputDto>(
        IQueryable<TEntity> query,
        PagedAndSortedResultRequestDto input,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryableAction = null, Func<List<TEntity>, Task<List<TEntity>>> entityAction = null)
    {
        return GetPagedListAsync<TEntity, TOuputDto>(query, input.MaxResultCount, input.SkipCount, input.Sorting, queryableAction, entityAction);
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

    protected virtual void TryToSetLastModificationTime<T>(T entity)
    {
        if (entity is IHasModificationTime)
        {
            var propertyInfo = entity.GetType().GetProperty(nameof(IHasModificationTime.LastModificationTime));

            if (propertyInfo == null || propertyInfo.GetSetMethod(true) == null)
            {
                return;
            }

            propertyInfo.SetValue(entity, Clock.Now);
        }
    }
}
