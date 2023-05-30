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
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using IczpNet.Chat.ChatObjects;
using Microsoft.AspNetCore.Authorization;

namespace IczpNet.Chat.BaseAppServices
{
    public abstract class CrudByChatObjectChatAppService<
        TEntity,
        TGetOutputDto,
        TGetListOutputDto,
        TKey,
        TGetListInput,
        TCreateInput,
        TUpdateInput>
        :
        CrudChatAppService<
            TEntity,
            TGetOutputDto,
            TGetListOutputDto,
            TKey,
            TGetListInput,
            TCreateInput,
            TUpdateInput>,
        ICrudByChatObjectChatAppService<
            TGetOutputDto,
            TGetListOutputDto,
            TKey,
            TGetListInput,
            TCreateInput,
            TUpdateInput>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TGetOutputDto : IEntityDto<TKey>
        where TGetListOutputDto : IEntityDto<TKey>

    {
        protected virtual string DeleteManyPolicyName { get; set; }

        //protected IRepository<Session, Guid> SessionRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<Session, Guid>>();
        //protected ISessionUnitManager MenuManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();

        protected ISessionPermissionChecker SessionPermissionChecker => LazyServiceProvider.LazyGetRequiredService<ISessionPermissionChecker>();

        protected IChatObjectManager ChatObjectManager => LazyServiceProvider.LazyGetRequiredService<IChatObjectManager>();

        protected CrudByChatObjectChatAppService(IRepository<TEntity, TKey> repository) : base(repository)
        {
        }

        protected virtual Task<IQueryable<TEntity>> CreateFilteredQueryAsync(ChatObject owner, TGetListInput input)
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

        //[Obsolete("CheckPolicyAsync(string policyName, ChatObject owner)", true)]
        protected override Task CheckPolicyAsync(string policyName)
        {
            throw new Exception("CheckPolicyAsync(string policyName, ChatObject owner)");
        }

        //protected virtual async Task CheckPolicyAsync(string policyName, ChatObject owner)
        //{
        //    //await AuthorizationService.CheckAsync(owner, policyName);

        //    await SessionPermissionChecker.CheckLoginAsync(owner);

        //    if (!string.IsNullOrWhiteSpace(policyName))
        //    {
        //        await SessionPermissionChecker.CheckAsync(policyName, owner);
        //    }
        //}

        protected virtual Task CheckGetPolicyAsync(ChatObject owner, TEntity entity)
        {
            return CheckPolicyAsync(GetPolicyName, owner);
        }

        protected virtual Task CheckGetListPolicyAsync(ChatObject owner, TGetListInput input)
        {
            return CheckPolicyAsync(GetListPolicyName, owner);
        }

        protected virtual Task CheckCreatePolicyAsync(ChatObject owner, TCreateInput input)
        {
            return CheckPolicyAsync(CreatePolicyName, owner);
        }

        protected virtual Task CheckUpdatePolicyAsync(ChatObject owner, TEntity entity, TUpdateInput input)
        {
            return CheckPolicyAsync(UpdatePolicyName, owner);
        }

        protected virtual Task CheckDeletePolicyAsync(ChatObject owner, TEntity entity)
        {
            return CheckPolicyAsync(DeletePolicyName, owner);
        }

        protected virtual Task CheckDeleteManyPolicyAsync(ChatObject owner, List<TKey> idList)
        {
            return CheckPolicyAsync(DeleteManyPolicyName, owner);
        }

        protected virtual async Task<ChatObject> GetAndCheckChatObjectAsync(long ownerId)
        {
            var owner = await ChatObjectManager.GetAsync(ownerId);

            Assert.If(!owner.IsEnabled, $"ChatObject disabled,ChatObjectId:{ownerId}");

            return owner;
        }

        [HttpGet]
        public virtual async Task<TGetOutputDto> GetAsync(long ownerId, TKey id)
        {
            //await SessionPermissionChecker.CheckAsync(GetPolicyName, ownerId);

            var owner = await GetAndCheckChatObjectAsync(ownerId);

            var entity = await base.GetEntityByIdAsync(id);

            await CheckGetPolicyAsync(owner, entity);

            return await MapToGetOutputDtoAsync(entity);
        }


        [HttpGet]
        public virtual async Task<PagedResultDto<TGetListOutputDto>> GetListAsync(long ownerId, TGetListInput input)
        {
            var owner = await GetAndCheckChatObjectAsync(ownerId);

            await CheckGetListPolicyAsync(owner, input);

            var query = await CreateFilteredQueryAsync(owner, input);

            var totalCount = await AsyncExecuter.CountAsync(query);

            var entityDtos = new List<TGetListOutputDto>();

            if (totalCount > 0)
            {
                query = ApplySorting(query, input);

                query = ApplyPaging(query, input);

                List<TEntity> entities = await AsyncExecuter.ToListAsync(query);

                entityDtos = await MapToGetListOutputDtosAsync(entities);
            }

            return new PagedResultDto<TGetListOutputDto>(
                totalCount,
                entityDtos
            );
        }

        [HttpPost]
        public virtual async Task<TGetOutputDto> CreateAsync(long ownerId, TCreateInput input)
        {
            var owner = await GetAndCheckChatObjectAsync(ownerId);

            return await CreateAsync(owner, input);
        }

        protected virtual async Task<TGetOutputDto> CreateAsync(ChatObject owner, TCreateInput input)
        {
            await CheckCreatePolicyAsync(owner, input);

            var entity = await MapToEntityAsync(owner, input);

            await CheckCreateAsync(input);

            await SetCreateEntityAsync(owner, entity, input);

            TryToSetTenantId(entity);

            await Repository.InsertAsync(entity, autoSave: true);

            //await base.CreateAsync(input);

            return await MapToGetOutputDtoAsync(entity);
        }

        protected virtual Task<TEntity> MapToEntityAsync(ChatObject owner, TCreateInput input)
        {
            return base.MapToEntityAsync(input);
        }

        protected virtual Task SetCreateEntityAsync(ChatObject owner, TEntity entity, TCreateInput input)
        {
            return base.SetCreateEntityAsync(entity, input);
        }

        [HttpPost]
        public virtual async Task<TGetOutputDto> UpdateAsync(long ownerId, TKey id, TUpdateInput input)
        {
            var owner = await GetAndCheckChatObjectAsync(ownerId);

            var entity = await GetEntityByIdAsync(id);

            await CheckUpdatePolicyAsync(owner, entity, input);

            await CheckUpdateAsync(id, entity, input);

            //TODO: Check if input has id different than given id and normalize if it's default value, throw ex otherwise
            await MapToEntityAsync(owner, input, entity);

            await SetUpdateEntityAsync(entity, input);

            await Repository.UpdateAsync(entity, autoSave: true);

            return await MapToGetOutputDtoAsync(entity);

        }

        private async Task MapToEntityAsync(ChatObject owner, TUpdateInput input, TEntity entity)
        {
            await MapToEntityAsync(input, entity);
        }

        [HttpPost]
        public virtual async Task DeleteByAsync(long ownerId, TKey id)
        {
            var owner = await GetAndCheckChatObjectAsync(ownerId);

            var entity = await Repository.GetAsync(id);

            await CheckDeletePolicyAsync(owner, entity);

            await base.DeleteAsync(id);
        }


        [HttpPost]
        public virtual async Task DeleteManyAsync(long ownerId, List<TKey> idList)
        {
            var owner = await GetAndCheckChatObjectAsync(ownerId);

            var predicate = GetPredicateDeleteManyAsync(owner);

            var entityIdList = (await Repository.GetQueryableAsync())
               .Where(x => idList.Contains(x.Id))
               .WhereIf(predicate != null, predicate)
               .Select(x => x.Id)
               .ToList();

            var notfindIdList = idList.Except(entityIdList).ToList();

            Assert.If(notfindIdList.Any(), $"not find {notfindIdList.Count}:[{notfindIdList.JoinAsString(",")}]");

            await CheckDeleteManyPolicyAsync(owner, idList);

            await DeleteManyAsync(idList);
        }

        protected virtual Expression<Func<TEntity, bool>> GetPredicateDeleteManyAsync(ChatObject owner)
        {
            return null;
        }
    }
}
