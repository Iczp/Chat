using Castle.Components.DictionaryAdapter.Xml;
using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Permissions;
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

namespace IczpNet.Chat.ChatObjects
{
    /// <inheritdoc cref="IChatObjectAppService"/>
    public class ChatObjectAppService
        : CrudTreeChatAppService<
            ChatObject,
            long,
            ChatObjectDto,
            ChatObjectDto,
            ChatObjectGetListInput,
            ChatObjectCreateInput,
            ChatObjectUpdateInput,
            ChatObjectInfo>,
        IChatObjectAppService
    {
        protected override string GetPolicyName { get; set; } = ChatPermissions.ChatObjectPermission.GetItem;
        protected override string GetListPolicyName { get; set; } = ChatPermissions.ChatObjectPermission.GetList;
        protected override string CreatePolicyName { get; set; } = ChatPermissions.ChatObjectPermission.Create;
        protected override string UpdatePolicyName { get; set; } = ChatPermissions.ChatObjectPermission.Update;
        protected override string DeletePolicyName { get; set; } = ChatPermissions.ChatObjectPermission.Delete;
        protected IChatObjectManager ChatObjectManager { get; }
        protected IChatObjectCategoryManager ChatObjectCategoryManager { get; }
        protected ISessionPermissionChecker SessionPermissionChecker { get; }

        public ChatObjectAppService(
            IChatObjectRepository repository,
            IChatObjectManager chatObjectManager,
            IChatObjectCategoryManager chatObjectCategoryManager,
            ISessionPermissionChecker sessionPermissionChecker) : base(repository, chatObjectManager)
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
                .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled)
                .WhereIf(input.IsDefault.HasValue, x => x.IsDefault == input.IsDefault)
                .WhereIf(input.IsPublic.HasValue, x => x.IsPublic == input.IsPublic)
                .WhereIf(input.IsStatic.HasValue, x => x.IsStatic == input.IsStatic)
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
            //owner.SetName(updateInput.Name);
            return base.MapToEntityAsync(updateInput, entity);
        }

        protected override ChatObject MapToEntity(ChatObjectCreateInput createInput)
        {
            var entity = base.MapToEntity(createInput);
            //owner.SetName(createInput.Name);
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
        public async Task<ChatObjectDto> UpdateNameAsync(long id, string name)
        {
            var entity = await ChatObjectManager.UpdateNameAsync(id, name);
            return await MapToGetOutputDtoAsync(entity);
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

            await ChatObjectManager.UpdateAsync(entity, entity.ParentId, isUnique: true);

            return await MapToGetOutputDtoAsync(entity);
        }

        [HttpPost]
        public virtual Task<ChatObjectDto> SetVerificationMethodAsync(long id, VerificationMethods verificationMethod)
        {
            return UpdateEntityAsync(id, entity => entity.SetVerificationMethod(verificationMethod));
        }

        protected virtual async Task<ChatObjectDetailDto> MapToEntityDetailAsync(ChatObject entity)
        {
            await Task.Yield();
            return ObjectMapper.Map<ChatObject, ChatObjectDetailDto>(entity);
        }

        [HttpGet]
        public async Task<ChatObjectDetailDto> GetDetailAsync(long id)
        {
            var entity = await ChatObjectManager.GetAsync(id);

            return await MapToEntityDetailAsync(entity);
        }

    }
}
