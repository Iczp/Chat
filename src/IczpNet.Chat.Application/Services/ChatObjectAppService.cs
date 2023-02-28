using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
{
    public class ChatObjectAppService
        : CrudTreeChatAppService<
            ChatObject,
            Guid,
            ChatObjectDetailDto,
            ChatObjectDto,
            ChatObjectGetListInput,
            ChatObjectCreateInput,
            ChatObjectUpdateInput,
            ChatObjectInfo>,
        IChatObjectAppService
    {
        protected IChatObjectManager ChatObjectManager { get; }
        protected IChatObjectCategoryManager ChatObjectCategoryManager { get; }
        public ChatObjectAppService(
            IRepository<ChatObject, Guid> repository,
            IChatObjectManager chatObjectManager,
            IChatObjectCategoryManager chatObjectCategoryManager) : base(repository)
        {
            ChatObjectManager = chatObjectManager;
            ChatObjectCategoryManager = chatObjectCategoryManager;
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
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword) || x.Code.Contains(input.Keyword))
                ;
        }

        [HttpGet]
        public virtual async Task<ChatObjectDetailDto> GetByAutoIdAsync(long autoId)
        {
            await CheckGetPolicyAsync();

            var entity = Assert.NotNull(await Repository.FindAsync(x => x.AutoId == autoId), $"Entity no such by [autoId]:{autoId}");

            return await MapToGetOutputDtoAsync(entity);
        }

        [HttpGet]
        public virtual async Task<ChatObjectDetailDto> GetByCodeAsync(string code)
        {
            await CheckGetPolicyAsync();

            var entity = Assert.NotNull(await Repository.FindAsync(x => x.Code == code), $"Entity no such by [code]:{code}");

            return await MapToGetOutputDtoAsync(entity);
        }

        [HttpGet]
        public virtual async Task<PagedResultDto<ChatObjectDto>> GetListByUserIdAsync(Guid userId, int maxResultCount = 10, int skipCount = 0, string sorting = null)
        {
            await CheckGetListPolicyAsync();

            var query = (await Repository.GetQueryableAsync()).Where(x => x.AppUserId == userId);

            return await GetPagedResultAsync(query, maxResultCount, skipCount, sorting);
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
        public override Task DeleteAsync(Guid id)
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

        //[RemoteService(false)]
        //public override Task DeleteManyAsync(List<Guid> idList)
        //{
        //    return base.DeleteManyAsync(idList);
        //}

        public async Task<ChatObjectDto> CreateRoomAsync(RoomCreateInput input)
        {
            var room = await ChatObjectManager.CreateRoomAsync(input.Name, input.ChatObjectIdList, input.OwnerId);

            return ObjectMapper.Map<ChatObject, ChatObjectDto>(room);
        }

    }
}
