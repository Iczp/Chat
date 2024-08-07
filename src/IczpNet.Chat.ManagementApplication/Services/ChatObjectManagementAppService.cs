﻿using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Management.BaseAppServices;
using IczpNet.Chat.Management.ChatObjects;
using IczpNet.Chat.Management.ChatObjects.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Users;

namespace IczpNet.Chat.Management.Services
{
    public class ChatObjectManagementAppService
        : CrudTreeChatManagementAppService<
            ChatObject,
            long,
            ChatObjectDto,
            ChatObjectDto,
            ChatObjectGetListInput,
            ChatObjectCreateInput,
            ChatObjectUpdateInput>,
        IChatObjectManagementAppService
    {
        protected IChatObjectManager ChatObjectManager { get; }
        protected override ITreeManager<ChatObject, long> TreeManager => LazyServiceProvider.LazyGetRequiredService<IChatObjectManager>();
        protected IChatObjectCategoryManager ChatObjectCategoryManager { get; }
        protected ISessionPermissionChecker SessionPermissionChecker { get; }
        public ChatObjectManagementAppService(
            IChatObjectRepository repository,
            IChatObjectCategoryManager chatObjectCategoryManager,
            IChatObjectManager chatObjectManager,
            ISessionPermissionChecker sessionPermissionChecker) : base(repository)
        {
            ChatObjectCategoryManager = chatObjectCategoryManager;
            ChatObjectManager = chatObjectManager;
            SessionPermissionChecker = sessionPermissionChecker;
        }

        protected override async Task<IQueryable<ChatObject>> CreateFilteredQueryAsync(ChatObjectGetListInput input)
        {
            //Category
            IQueryable<Guid> categoryIdQuery = null;

            if (input.IsImportChildCategory && input.CategoryIdList.IsAny())
            {
                categoryIdQuery = (await ChatObjectCategoryManager.QueryCurrentAndAllChildsAsync(input.CategoryIdList)).Select(x => x.Id);
            }
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(!input.ChatObjectTypeId.IsNullOrWhiteSpace(), x => x.ChatObjectTypeId == input.ChatObjectTypeId)
                .WhereIf(input.ObjectType.HasValue, x => x.ObjectType == input.ObjectType)
                //CategoryId
                .WhereIf(!input.IsImportChildCategory && input.CategoryIdList.IsAny(), x => x.ChatObjectCategoryUnitList.Any(d => input.CategoryIdList.Contains(d.CategoryId)))
                .WhereIf(input.IsImportChildCategory && input.CategoryIdList.IsAny(), x => x.ChatObjectCategoryUnitList.Any(d => categoryIdQuery.Contains(d.CategoryId)))
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword) || x.Code.Contains(input.Keyword) || x.NameSpellingAbbreviation.Contains(input.Keyword))
                ;
        }

        [HttpGet]
        public virtual async Task<ChatObjectDto> GetByCodeAsync(string code)
        {
            Assert.If(code.IsNullOrWhiteSpace(), $"[code] IsNullOrWhiteSpace.");

            await CheckGetPolicyAsync();

            var entity = Assert.NotNull(await ChatObjectManager.FindByCodeAsync(code), $"Entity no such by [code]:{code}");

            return await MapToGetOutputDtoAsync(entity);
        }

        [HttpGet]
        public virtual async Task<PagedResultDto<ChatObjectDto>> GetListByUserIdAsync(Guid userId, int maxResultCount = 10, int skipCount = 0, string sorting = null)
        {
            await CheckGetListPolicyAsync();

            var query = (await Repository.GetQueryableAsync()).Where(x => x.AppUserId == userId);

            return await GetPagedResultAsync(query, maxResultCount, skipCount, sorting);
        }

        [HttpGet]
        [Authorize]
        public virtual Task<PagedResultDto<ChatObjectDto>> GetListByCurrentUserAsync(int maxResultCount = 10, int skipCount = 0, string sorting = null)
        {
            return GetListByUserIdAsync(CurrentUser.GetId(), maxResultCount, skipCount, sorting);
        }

        protected virtual async Task<PagedResultDto<ChatObjectDto>> GetPagedResultAsync(IQueryable<ChatObject> query, int maxResultCount = 10, int skipCount = 0, string sorting = null)
        {
            var totalCount = await AsyncExecuter.CountAsync(query);

            if (!sorting.IsNullOrWhiteSpace())
            {
                query = query.OrderBy(sorting);
            }

            query = query.PageBy(skipCount, maxResultCount);

            var entities = await AsyncExecuter.ToListAsync(query);

            var items = ObjectMapper.Map<List<ChatObject>, List<ChatObjectDto>>(entities);

            return new PagedResultDto<ChatObjectDto>(totalCount, items);
        }

        [RemoteService(false)]
        public override Task DeleteAsync(long id)
        {
            return base.DeleteAsync(id);
        }

        protected override Task MapToEntityAsync(ChatObjectUpdateInput updateInput, ChatObject entity)
        {
            entity.SetName(updateInput.Name);
            return base.MapToEntityAsync(updateInput, entity);
        }

        protected override ChatObject MapToEntity(ChatObjectCreateInput createInput)
        {
            var entity = base.MapToEntity(createInput);
            entity.SetName(createInput.Name);
            return entity;
        }



        [HttpPost]
        public virtual async Task<ChatObjectDto> CreateShopKeeperAsync(string name)
        {
            var shopKeeper = await ChatObjectManager.CreateShopKeeperAsync(name);

            return ObjectMapper.Map<ChatObject, ChatObjectDto>(shopKeeper);
        }

        [HttpPost]
        public virtual async Task<ChatObjectDto> CreateShopWaiterAsync(long shopKeeperId, string name)
        {
            var shopWaiter = await ChatObjectManager.CreateShopWaiterAsync(shopKeeperId, name);

            return ObjectMapper.Map<ChatObject, ChatObjectDto>(shopWaiter);
        }

        [HttpPost]
        public virtual async Task<ChatObjectDto> CreateRobotAsync(string name)
        {
            var entity = await ChatObjectManager.CreateRobotAsync(name);

            return ObjectMapper.Map<ChatObject, ChatObjectDto>(entity);
        }

        [HttpPost]
        public virtual async Task<ChatObjectDto> CreateSquareAsync(string name)
        {
            var entity = await ChatObjectManager.CreateSquareAsync(name);

            return ObjectMapper.Map<ChatObject, ChatObjectDto>(entity);
        }

        [HttpPost]
        public Task<ChatObjectDto> UpdateNameAsync(long id, string name)
        {
            return UpdateEntityAsync(id, entity => entity.SetName(name));
        }

        [HttpPost]
        [RemoteService(false)]
        public Task<ChatObjectDto> UpdatePortraitAsync(long id, string portrait)
        {
            return UpdateEntityAsync(id, entity => entity.SetPortrait(portrait));
        }

        protected virtual async Task<ChatObjectDto> UpdateEntityAsync(long id, Action<ChatObject> action)
        {
            var entity = await ChatObjectManager.GetAsync(id);

            action?.Invoke(entity);

            await ChatObjectManager.UpdateAsync(entity, isUnique: true);

            return await MapToGetOutputDtoAsync(entity);
        }

        [HttpPost]
        public virtual Task<ChatObjectDto> SetVerificationMethodAsync(long id, VerificationMethods verificationMethod)
        {
            return UpdateEntityAsync(id, entity => entity.SetVerificationMethod(verificationMethod));
        }
    }
}
