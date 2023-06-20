using IczpNet.AbpTrees;
using IczpNet.AbpTrees.Dtos;
using IczpNet.Chat.ChatObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.BaseAppServices
{
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

        protected virtual async Task<PagedResultDto<TOuputDto>> GetPagedListAsync<T, TOuputDto>(
        IQueryable<T> query,
        int maxResultCount = 10,
        int skipCount = 0, string sorting = null,
        Func<IQueryable<T>, IQueryable<T>> queryableAction = null, Func<List<T>, Task<List<T>>> entityAction = null)
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

            var items = ObjectMapper.Map<List<T>, List<TOuputDto>>(entities);

            return new PagedResultDto<TOuputDto>(totalCount, items);
        }

        protected virtual Task<PagedResultDto<TOuputDto>> GetPagedListAsync<T, TOuputDto>(
            IQueryable<T> query,
            PagedAndSortedResultRequestDto input,
            Func<IQueryable<T>, IQueryable<T>> queryableAction = null, Func<List<T>, Task<List<T>>> entityAction = null)
        {
            return GetPagedListAsync<T, TOuputDto>(query, input.MaxResultCount, input.SkipCount, input.Sorting, queryableAction, entityAction);
        }


        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet]
        public override Task<PagedResultDto<TTreeInfo>> GetAllByCacheAsync(TreeGetListInput<TKey> input)
        {
            return base.GetAllByCacheAsync(input);
        }
    }


}
