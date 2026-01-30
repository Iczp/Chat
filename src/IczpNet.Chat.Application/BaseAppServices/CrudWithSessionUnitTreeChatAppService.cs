using IczpNet.AbpTrees;
using IczpNet.AbpTrees.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissions;
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
using Microsoft.AspNetCore.Authorization;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.Sessions;

namespace IczpNet.Chat.BaseAppServices;

[ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
[Authorize]
public abstract class CrudWithSessionUnitTreeChatAppService<
    TEntity,
    TKey,
    TGetOutputDto,
    TGetListOutputDto,
    TGetListInput,
    TCreateInput,
    TUpdateInput,
    TTreeInfo>
    :
    CrudTreeChatAppService<
        TEntity,
        TKey,
        TGetOutputDto,
        TGetListOutputDto,
        TGetListInput,
        TCreateInput,
        TUpdateInput,
        TTreeInfo>
    where TKey : struct
    where TEntity : class, ITreeEntity<TEntity, TKey>, ISessionId
    where TGetOutputDto : IEntityDto<TKey>
    where TGetListOutputDto : IEntityDto<TKey>
    where TGetListInput : ITreeGetListInput<TKey>, ISessionId
    where TCreateInput : ITreeInput<TKey>, ISessionId
    where TUpdateInput : ITreeInput<TKey>
    where TTreeInfo : ITreeInfo<TKey>
{

    protected virtual string GetBySessionUnitPolicyName { get; set; }
    protected virtual string GetListBySessionUnitPolicyName { get; set; }
    protected virtual string CreateBySessionUnitPolicyName { get; set; }
    protected virtual string UpdateBySessionUnitPolicyName { get; set; }
    protected virtual string DeleteBySessionUnitPolicyName { get; set; }
    protected virtual string DeleteManyBySessionUnitPolicyName { get; set; }
    protected IRepository<Session, Guid> SessionRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<Session, Guid>>();
    protected ISessionPermissionChecker SessionPermissionChecker => LazyServiceProvider.LazyGetRequiredService<ISessionPermissionChecker>();
    //protected ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();

    protected CrudWithSessionUnitTreeChatAppService(
        IRepository<TEntity, TKey> repository,
        ITreeManager<TEntity, TKey, TTreeInfo> treeManager) : base(repository, treeManager)
    {
    }

    protected virtual void TryToSetSessionId<T>(T entity, Guid? sessionId) where T : ISessionId
    {
        var propertyInfo = entity.GetType().GetProperty(nameof(ISessionId.SessionId));

        if (entity is ISessionId && propertyInfo != null && propertyInfo.GetSetMethod(true) != null)
        {
            propertyInfo.SetValue(entity, sessionId);
        }
    }

    protected virtual async Task<SessionUnit> GetAndCheckSessionUnitAsync(Guid sessionUnitId)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        Assert.If(!sessionUnit.Setting.IsEnabled, $"SessionUnit disabled,SessionUnitId:{sessionUnit.Id}");

        return sessionUnit;
    }

    protected virtual async Task CheckGetBySessionUnitAsync(SessionUnit sessionUnit, TEntity entity)
    {
        Assert.If(sessionUnit.SessionId != entity.SessionId, $"Not in same session");

        await SessionPermissionChecker.CheckAsync(GetBySessionUnitPolicyName, sessionUnit);
    }

    [HttpGet]
    public virtual async Task<TGetOutputDto> GetByAsync(Guid sessionUnitId, TKey id)
    {
        //await SessionPermissionChecker.CheckAsync(GetPolicyName, sessionUnitId);

        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        var entity = await base.GetEntityByIdAsync(id);

        await CheckGetBySessionUnitAsync(sessionUnit, entity);

        return await MapToGetOutputDtoAsync(entity);
    }

    protected virtual async Task CheckGetListBySessionUnitAsync(SessionUnit sessionUnit, TGetListInput input)
    {
        Assert.If(input.SessionId.HasValue && sessionUnit.SessionId != input.SessionId, $"Not in same session");

        await SessionPermissionChecker.CheckAsync(GetListBySessionUnitPolicyName, sessionUnit);
    }

    [HttpGet]
    public virtual async Task<PagedResultDto<TGetListOutputDto>> GetListByAsync(Guid sessionUnitId, TGetListInput input)
    {
        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        await CheckGetListBySessionUnitAsync(sessionUnit, input);

        TryToSetSessionId(input, sessionUnit.SessionId);

        GetListPolicyName = string.Empty;

        return await base.GetListAsync(input);
    }

    protected virtual async Task CheckCreateBySessionUnitAsync(SessionUnit sessionUnit, TCreateInput input)
    {
        Assert.If(input.SessionId.HasValue && sessionUnit.SessionId != input.SessionId, $"Not in same session");

        await SessionPermissionChecker.CheckAsync(CreateBySessionUnitPolicyName, sessionUnit);
    }

    [HttpPost]
    public virtual async Task<TGetOutputDto> CreateByAsync(Guid sessionUnitId, TCreateInput input)
    {
        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        await CheckCreateBySessionUnitAsync(sessionUnit, input);

        TryToSetSessionId(input, sessionUnit.SessionId);

        CreatePolicyName = string.Empty;

        return await base.CreateAsync(input);
    }

    protected virtual async Task CheckUpdateBySessionUnitAsync(SessionUnit sessionUnit, TKey id, TUpdateInput input)
    {
        await SessionPermissionChecker.CheckAsync(UpdateBySessionUnitPolicyName, sessionUnit);
    }

    [HttpPost]
    public virtual async Task<TGetOutputDto> UpdateByAsync(Guid sessionUnitId, TKey id, TUpdateInput input)
    {
        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        await CheckUpdateBySessionUnitAsync(sessionUnit, id, input);

        UpdatePolicyName = string.Empty;

        return await base.UpdateAsync(id, input);
    }

    protected virtual async Task CheckDeleteBySessionUnitAsync(SessionUnit sessionUnit, TEntity entity)
    {
        Assert.If(sessionUnit.SessionId != entity.SessionId, $"Not in same session");

        await SessionPermissionChecker.CheckAsync(DeleteBySessionUnitPolicyName, sessionUnit);
    }

    [HttpPost]
    public virtual async Task DeleteByAsync(Guid sessionUnitId, TKey id)
    {
        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        var entity = await Repository.GetAsync(id);

        await CheckDeleteBySessionUnitAsync(sessionUnit, entity);

        DeletePolicyName = string.Empty;

        await base.DeleteAsync(id);
    }

    protected virtual async Task CheckDeleteManyBySessionUnitAsync(SessionUnit sessionUnit, List<TKey> idList)
    {
        await SessionPermissionChecker.CheckAsync(DeleteManyBySessionUnitPolicyName, sessionUnit);
    }

    [HttpPost]
    public virtual async Task DeleteManyByAsync(Guid sessionUnitId, List<TKey> idList)
    {
        var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        var predicate = GetPredicateDeleteManyByAsync(sessionUnit);

        var entityIdList = (await Repository.GetQueryableAsync())
           .Where(x => idList.Contains(x.Id))
           .WhereIf(predicate != null, predicate)
           .Select(x => x.Id)
           .ToList();

        var notfindIdList = idList.Except(entityIdList).ToList();

        Assert.If(notfindIdList.Any(), $"not find {notfindIdList.Count}:[{notfindIdList.JoinAsString(",")}]");

        await CheckDeleteManyBySessionUnitAsync(sessionUnit, idList);

        DeletePolicyName = string.Empty;

        await DeleteManyAsync(idList);
    }

    protected virtual Expression<Func<TEntity, bool>> GetPredicateDeleteManyByAsync(SessionUnit sessionUnit)
    {
        return x => x.SessionId == sessionUnit.SessionId;
    }
}
