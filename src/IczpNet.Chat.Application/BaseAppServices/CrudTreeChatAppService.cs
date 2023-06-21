using IczpNet.AbpCommons.Extensions;
using IczpNet.AbpTrees;
using IczpNet.AbpTrees.Dtos;
using IczpNet.Chat.ChatObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.BaseAppServices;

public abstract class CrudTreeChatAppService<
    TEntity,
    TKey,
    TGetOutputDto,
    TGetListOutputDto,
    TGetListInput,
    TCreateInput,
    TUpdateInput>
    :
    CrudTreeChatAppService<
        TEntity,
        TKey,
        TGetOutputDto,
        TGetListOutputDto,
        TGetListInput,
        TCreateInput,
        TUpdateInput,
        TreeInfo<TKey>>
    where TKey : struct
    where TEntity : class, ITreeEntity<TEntity, TKey>
    where TGetOutputDto : IEntityDto<TKey>
    where TGetListOutputDto : IEntityDto<TKey>
    where TGetListInput : ITreeGetListInput<TKey>
    where TCreateInput : ITreeInput<TKey>
    where TUpdateInput : ITreeInput<TKey>
{
    protected CrudTreeChatAppService(
        IRepository<TEntity, TKey> repository,
        ITreeManager<TEntity, TKey, TreeInfo<TKey>> treeManager) : base(repository, treeManager)
    {

    }
}


[ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
[Authorize]
public abstract class CrudTreeChatAppService<
    TEntity,
    TKey,
    TGetOutputDto,
    TGetListOutputDto,
    TGetListInput,
    TCreateInput,
    TUpdateInput,
    TTreeInfo>
    :
    TreeAppService<
        TEntity,
        TKey,
        TGetOutputDto,
        TGetListOutputDto,
        TGetListInput,
        TCreateInput,
        TUpdateInput,
        TTreeInfo>
    ,
    ITreeAppService<TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput, TTreeInfo>
    where TKey : struct
    where TEntity : class, ITreeEntity<TEntity, TKey>
    where TGetOutputDto : IEntityDto<TKey>
    where TGetListOutputDto : IEntityDto<TKey>
    where TGetListInput : ITreeGetListInput<TKey>
    where TCreateInput : ITreeInput<TKey>
    where TUpdateInput : ITreeInput<TKey>
    where TTreeInfo : ITreeInfo<TKey>
{
    protected ICurrentChatObject CurrentChatObject => LazyServiceProvider.LazyGetRequiredService<ICurrentChatObject>();
    protected CrudTreeChatAppService(
        IRepository<TEntity, TKey> repository,
        ITreeManager<TEntity, TKey, TTreeInfo> treeManager) : base(repository, treeManager)
    {

    }

    protected virtual async Task<PagedResultDto<TOuputDto>> GetPagedListAsync<T, TOuputDto>(
        IQueryable<T> query,
        PagedAndSortedResultRequestDto input,
        Func<IQueryable<T>, IQueryable<T>> queryableAction = null,
        Func<List<T>, Task<List<T>>> entityAction = null)
    {
        await CheckPolicyAsync(GetListPolicyName);

        return await query.ToPagedListAsync<T, TOuputDto>(AsyncExecuter, ObjectMapper, input, queryableAction, entityAction);
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

    #region 重写备注

    /// <summary>
    /// 列表(缓存)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public override Task<PagedResultDto<TTreeInfo>> GetAllByCacheAsync(TreeGetListInput<TKey> input)
    {
        return base.GetAllByCacheAsync(input);
    }

    /// <summary>
    /// 列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public override Task<PagedResultDto<TGetListOutputDto>> GetListAsync(TGetListInput input)
    {
        return base.GetListAsync(input);
    }

    /// <summary>
    /// 获取一条数据
    /// </summary>
    /// <param name="id">主键Id</param>
    /// <returns></returns>
    [HttpGet]
    public override Task<TGetOutputDto> GetAsync(TKey id)
    {
        return base.GetAsync(id);
    }

    /// <summary>
    /// 获取一条数据(缓存)
    /// </summary>
    /// <param name="id">主键Id</param>
    /// <returns></returns>
    [HttpGet]
    public override Task<TTreeInfo> GetItemByCacheAsync(TKey id)
    {
        return base.GetItemByCacheAsync(id);
    }

    /// <summary>
    /// 获取多条数据
    /// </summary>
    /// <param name="idList">主键Id[多个]</param>
    /// <returns></returns>
    [HttpGet]
    public override Task<List<TGetOutputDto>> GetManyAsync(List<TKey> idList)
    {
        return base.GetManyAsync(idList);
    }

    /// <summary>
    /// 获取多条数据(缓存)
    /// </summary>
    /// <param name="idList">主键Id[多个]</param>
    /// <returns></returns>
    [HttpGet]
    public override Task<List<TTreeInfo>> GetManayByCacheAsync(List<TKey> idList)
    {
        return base.GetManayByCacheAsync(idList);
    }
    /// <summary>
    /// 修改
    /// </summary>
    /// <param name="id">主键Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public override Task<TGetOutputDto> UpdateAsync(TKey id, TUpdateInput input)
    {
        return base.UpdateAsync(id, input);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public override Task<TGetOutputDto> CreateAsync(TCreateInput input)
    {
        return base.CreateAsync(input);
    }

    /// <summary>
    /// 删除一条数据
    /// </summary>
    /// <param name="id">主键Id</param>
    /// <returns></returns>
    [HttpPost]
    public override Task DeleteAsync(TKey id)
    {
        return base.DeleteAsync(id);
    }

    /// <summary>
    /// 删除多条数据
    /// </summary>
    /// <param name="idList">主键Id[多个]</param>
    /// <returns></returns>
    [HttpPost]
    public override Task DeleteManyAsync(List<TKey> idList)
    {
        return base.DeleteManyAsync(idList);
    }

    /// <summary>
    /// 修复数据（fullPath,fullName,childrenCount,depth等）
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public override Task<DateTime> RepairDataAsync()
    {
        return base.RepairDataAsync();
    }
    #endregion
}
