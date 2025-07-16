using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.Sessions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using IczpNet.AbpCommons;
using System.Linq;
using System.Linq.Expressions;
using IczpNet.Chat.SessionSections;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using IczpNet.Chat.SessionUnits;

namespace IczpNet.Chat.BaseAppServices;

[ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
[Authorize]
public abstract class CrudBySessionUnitChatAppService<
    TEntity,
    TGetOutputDto,
    TGetListOutputDto,
    TKey,
    TGetListInput,
    TCreateInput,
    TUpdateInput>(IRepository<TEntity, TKey> repository)
    :
    CrudChatAppService<
        TEntity,
        TGetOutputDto,
        TGetListOutputDto,
        TKey,
        TGetListInput,
        TCreateInput,
        TUpdateInput>(repository)
    where TKey : struct
    where TEntity : class, IEntity<TKey>
    where TGetOutputDto : IEntityDto<TKey>
    where TGetListOutputDto : IEntityDto<TKey>

{

    protected virtual string DeleteManyPolicyName { get; set; }
    protected IRepository<Session, Guid> SessionRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<Session, Guid>>();
    protected ISessionPermissionChecker SessionPermissionChecker => LazyServiceProvider.LazyGetRequiredService<ISessionPermissionChecker>();

    protected virtual Task<IQueryable<TEntity>> CreateFilteredQueryAsync(SessionUnit sessionUnit, TGetListInput input)
    {
        return base.CreateFilteredQueryAsync(input);
    }

    protected virtual void TryToSetSessionId<T>(T entity, Guid? sessionId) //where T : ISessionId
    {
        var propertyInfo = entity.GetType().GetProperty(nameof(ISessionId.SessionId));

        if (entity is ISessionId && propertyInfo != null && propertyInfo.GetSetMethod(true) != null)
        {
            propertyInfo.SetValue(entity, sessionId);
        }
    }

    [RemoteService(false)]
    public override Task<TGetOutputDto> CreateAsync(TCreateInput input) => base.CreateAsync(input);
    [RemoteService(false)]
    public override Task<TGetOutputDto> UpdateAsync(TKey id, TUpdateInput input) => base.UpdateAsync(id, input);

    [RemoteService(false)]
    public override Task<TGetOutputDto> GetAsync(TKey id) => base.GetAsync(id);

    [RemoteService(false)]
    public override Task<PagedResultDto<TGetListOutputDto>> GetListAsync(TGetListInput input) => base.GetListAsync(input);

    [RemoteService(false)]
    public override Task DeleteAsync(TKey id) => base.DeleteAsync(id);

    [RemoteService(false)]
    public override Task DeleteManyAsync(List<TKey> idList) => base.DeleteManyAsync(idList);

    //[Obsolete("CheckPolicyAsync(string policyName, SessionUnit sessionUnit)", true)]
    protected override Task CheckPolicyAsync(string policyName)
    {
        throw new Exception("CheckPolicyAsync(string policyName, SessionUnit sessionUnit)");
    }

    //protected virtual async Task CheckPolicyAsync(string policyName, SessionUnit sessionUnit)
    //{
    //    await SessionPermissionChecker.CheckAsync(policyName, sessionUnit);
    //}

    protected virtual Task CheckGetPolicyAsync(SessionUnit sessionUnit, TEntity entity)
    {
        return CheckPolicyAsync(GetPolicyName, sessionUnit);
    }

    protected virtual Task CheckGetListPolicyAsync(SessionUnit sessionUnit, TGetListInput input)
    {
        return CheckPolicyAsync(GetListPolicyName, sessionUnit);
    }

    protected virtual Task CheckCreatePolicyAsync(SessionUnit sessionUnit, TCreateInput input)
    {
        return CheckPolicyAsync(CreatePolicyName, sessionUnit);
    }

    protected virtual Task CheckUpdatePolicyAsync(SessionUnit sessionUnit, TEntity entity, TUpdateInput input)
    {
        return CheckPolicyAsync(UpdatePolicyName, sessionUnit);
    }

    protected virtual Task CheckDeletePolicyAsync(SessionUnit sessionUnit, TEntity entity)
    {
        return CheckPolicyAsync(DeletePolicyName, sessionUnit);
    }

    protected virtual Task CheckDeleteManyPolicyAsync(SessionUnit sessionUnit, List<TKey> idList)
    {
        return CheckPolicyAsync(DeleteManyPolicyName, sessionUnit);
    }

    protected virtual async Task<SessionUnit> GetAndCheckSessionUnitAsync(Guid sessionUnitId)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        Assert.If(!sessionUnit.Setting.IsEnabled, $"SessionUnit disabled,SessionUnitId:{sessionUnit.Id}");

        return sessionUnit;
    }


    [HttpGet]
    public virtual async Task<TGetOutputDto> GetAsync(Guid sessionUnitId, TKey id)
    {
        //await SessionPermissionChecker.CheckAsync(GetPolicyName, sessionUnitId);

        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        var entity = await base.GetEntityByIdAsync(id);

        await CheckGetPolicyAsync(sessionUnit, entity);

        return await MapToGetOutputDtoAsync(entity);
    }


    [HttpGet]
    public virtual async Task<PagedResultDto<TGetListOutputDto>> GetListAsync(Guid sessionUnitId, TGetListInput input)
    {
        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        await CheckGetListPolicyAsync(sessionUnit, input);

        var query = await CreateFilteredQueryAsync(sessionUnit, input);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var entityDtos = new List<TGetListOutputDto>();

        if (totalCount > 0)
        {
            query = ApplySorting(query, input);

            query = ApplyPaging(query, input);

            List<TEntity> entities = await AsyncExecuter.ToListAsync(query);

            entityDtos = await MapToGetListOutputDtosAsync(entities);
        }

        return new PagedResultDto<TGetListOutputDto>(totalCount, entityDtos);
    }

    [HttpPost]
    public virtual async Task<TGetOutputDto> CreateAsync(Guid sessionUnitId, TCreateInput input)
    {
        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await CreateAsync(sessionUnit, input);
    }

    protected virtual async Task<TGetOutputDto> CreateAsync(SessionUnit sessionUnit, TCreateInput input)
    {
        await CheckCreatePolicyAsync(sessionUnit, input);

        var entity = await MapToEntityAsync(sessionUnit, input);

        await CheckCreateAsync(input);

        await SetCreateEntityAsync(sessionUnit, entity, input);

        TryToSetTenantId(entity);

        await Repository.InsertAsync(entity, autoSave: true);

        //await base.CreateAsync(input);

        return await MapToGetOutputDtoAsync(entity);
    }

    protected virtual Task<TEntity> MapToEntityAsync(SessionUnit sessionUnit, TCreateInput input)
    {
        return base.MapToEntityAsync(input);
    }

    protected virtual Task SetCreateEntityAsync(SessionUnit sessionUnit, TEntity entity, TCreateInput input)
    {
        return base.SetCreateEntityAsync(entity, input);
    }

    [HttpPost]
    public virtual async Task<TGetOutputDto> UpdateAsync(Guid sessionUnitId, TKey id, TUpdateInput input)
    {
        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        var entity = await GetEntityByIdAsync(id);

        await CheckUpdatePolicyAsync(sessionUnit, entity, input);

        await CheckUpdateAsync(id, entity, input);

        //TODO: Check if input has id different than given id and normalize if it's default value, throw ex otherwise
        await MapToEntityAsync(sessionUnit, input, entity);

        await SetUpdateEntityAsync(entity, input);

        await Repository.UpdateAsync(entity, autoSave: true);

        return await MapToGetOutputDtoAsync(entity);

    }

    private async Task MapToEntityAsync(SessionUnit sessionUnit, TUpdateInput input, TEntity entity)
    {
        await MapToEntityAsync(input, entity);
    }

    [HttpPost]
    public virtual async Task DeleteByAsync(Guid sessionUnitId, TKey id)
    {
        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        var entity = await Repository.GetAsync(id);

        await CheckDeletePolicyAsync(sessionUnit, entity);

        await base.DeleteAsync(id);
    }


    [HttpPost]
    public virtual async Task DeleteManyAsync(Guid sessionUnitId, List<TKey> idList)
    {
        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        var predicate = GetPredicateDeleteManyAsync(sessionUnit);

        var entityIdList = (await Repository.GetQueryableAsync())
           .Where(x => idList.Contains(x.Id))
           .WhereIf(predicate != null, predicate)
           .Select(x => x.Id)
           .ToList();

        var notfindIdList = idList.Except(entityIdList).ToList();

        Assert.If(notfindIdList.Any(), $"not find {notfindIdList.Count}:[{notfindIdList.JoinAsString(",")}]");

        await CheckDeleteManyPolicyAsync(sessionUnit, idList);

        await DeleteManyAsync(idList);
    }

    protected virtual Expression<Func<TEntity, bool>> GetPredicateDeleteManyAsync(SessionUnit sessionUnit)
    {
        return null;
    }
}
