using IczpNet.AbpTrees;
using IczpNet.AbpTrees.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.Sessions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.AbpCommons;
using System.Linq;
using System.Linq.Expressions;

namespace IczpNet.Chat.BaseAppServices
{
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
        where TEntity : class, ITreeEntity<TEntity, TKey>
        where TGetOutputDto : IEntityDto<TKey>
        where TGetListOutputDto : IEntityDto<TKey>
        where TGetListInput : ITreeGetListInput<TKey>
        where TCreateInput : ITreeInput<TKey>
        where TUpdateInput : ITreeInput<TKey>
        where TTreeInfo : ITreeInfo<TKey>
    {

        protected IRepository<Session, Guid> SessionRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<Session, Guid>>();
        protected ISessionPermissionChecker SessionPermissionChecker => LazyServiceProvider.LazyGetRequiredService<ISessionPermissionChecker>();
        protected ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();

        protected CrudWithSessionUnitTreeChatAppService(IRepository<TEntity, TKey> repository) : base(repository)
        {
        }

        protected virtual async Task<SessionUnit> GetAndCheckSessionUnitAsync(Guid sessionUnitId)
        {
            var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

            Assert.If(!sessionUnit.IsEnabled, $"SessionUnit disabled,SessionUnitId:{sessionUnit.Id}");

            return sessionUnit;
        }

        protected virtual async Task CheckGetByAsync(SessionUnit sessionUnit, TEntity entity)
        {
            await SessionPermissionChecker.CheckAsync(GetPolicyName, sessionUnit);
        }

        [HttpPost]
        public virtual async Task<TGetOutputDto> GetByAsync(Guid sessionUnitId, TKey id)
        {
            //await SessionPermissionChecker.CheckAsync(GetPolicyName, sessionUnitId);

            var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

            var entity = await base.GetEntityByIdAsync(id);

            await CheckGetByAsync(sessionUnit, entity);

            return await MapToGetOutputDtoAsync(entity);
        }

        protected virtual async Task CheckGetListByAsync(SessionUnit sessionUnit, TGetListInput input)
        {
            await SessionPermissionChecker.CheckAsync(GetListPolicyName, sessionUnit);
        }

        [HttpPost]
        public virtual async Task<PagedResultDto<TGetListOutputDto>> GetListByAsync(Guid sessionUnitId, TGetListInput input)
        {

            //Assert.If(!input.SessionId.HasValue, $"SessionId is null");

            var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

            //Assert.If(sessionUnit.SessionId != input.SessionId, $"SessionId is null");

            await CheckGetListByAsync(sessionUnit, input);

            GetListPolicyName = string.Empty;

            return await base.GetListAsync(input);
        }

        protected virtual async Task CheckCreateByAsync(SessionUnit sessionUnit, TCreateInput input)
        {
            await SessionPermissionChecker.CheckAsync(CreatePolicyName, sessionUnit);
        }

        [HttpPost]
        public virtual async Task<TGetOutputDto> CreateByAsync(Guid sessionUnitId, TCreateInput input)
        {
            var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

            await SessionPermissionChecker.CheckAsync(CreatePolicyName, sessionUnit);

            CreatePolicyName = string.Empty;

            return await base.CreateAsync(input);
        }

        protected virtual async Task CheckUpdateByAsync(SessionUnit sessionUnit, TKey id, TUpdateInput input)
        {
            await SessionPermissionChecker.CheckAsync(UpdatePolicyName, sessionUnit);
        }

        [HttpPost]
        public virtual async Task<TGetOutputDto> UpdateByAsync(Guid sessionUnitId, TKey id, TUpdateInput input)
        {
            var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

            await CheckUpdateByAsync(sessionUnit, id, input);

            UpdatePolicyName = string.Empty;

            return await base.UpdateAsync(id, input);
        }

        protected virtual async Task CheckDeleteByAsync(SessionUnit sessionUnit, TEntity entity)
        {
            await SessionPermissionChecker.CheckAsync(DeletePolicyName, sessionUnit);
        }

        [HttpPost]
        public virtual async Task DeleteByAsync(Guid sessionUnitId, TKey id)
        {
            var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

            var entity = await Repository.GetAsync(id);

            await CheckDeleteByAsync(sessionUnit, entity);

            DeletePolicyName = string.Empty;

            await base.DeleteAsync(id);
        }

        protected virtual async Task CheckDeleteManyByAsync(SessionUnit sessionUnit, List<TKey> idList)
        {
            await SessionPermissionChecker.CheckAsync(DeletePolicyName, sessionUnit);
        }

        [HttpPost]
        public virtual async Task DeleteManyByAsync(Guid sessionUnitId, List<TKey> idList)
        {
            var sessionUnit = await GetAndCheckSessionUnitAsync(sessionUnitId);

            var predicate = GetPredicateDeleteManyByAsync(sessionUnit);

            var entityIdList = (await Repository.GetQueryableAsync())
               .Where(x => idList.Contains(x.Id))
               .Where(predicate)
               .Select(x => x.Id)
               .ToList();

            var notfindIdList = idList.Except(entityIdList).ToList();

            Assert.If(notfindIdList.Any(), $"not find {notfindIdList.Count}:[{notfindIdList.JoinAsString(",")}]");

            await CheckDeleteManyByAsync(sessionUnit, idList);

            DeletePolicyName = string.Empty;

            await Repository.DeleteManyAsync(idList);
        }

        protected virtual Expression<Func<TEntity, bool>> GetPredicateDeleteManyByAsync(SessionUnit sessionUnit)
        {
            return x => true;
        }
    }
}
