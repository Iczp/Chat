using Castle.Components.DictionaryAdapter.Xml;
using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Dtos;
using IczpNet.AbpCommons.Extensions;
using IczpNet.AbpCommons.Models;
using IczpNet.AbpTrees;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjectEntryValues;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Comparers;
using IczpNet.Chat.EntryNames;
using IczpNet.Chat.EntryValues;
using IczpNet.Chat.EntryValues.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.SessionPermissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NUglify;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace IczpNet.Chat.Services
{
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
        protected IChatObjectManager ChatObjectManager { get; }
        protected override ITreeManager<ChatObject, long> TreeManager => LazyServiceProvider.LazyGetRequiredService<IChatObjectManager>();
        protected IChatObjectCategoryManager ChatObjectCategoryManager { get; }
        protected ISessionPermissionChecker SessionPermissionChecker { get; }

        protected IRepository<EntryName, Guid> EntryNameRepository { get; }
        protected IRepository<EntryValue, Guid> EntryValueRepository { get; }

        protected IRepository<ChatObjectTargetEntryValue> ChatObjectTargetEntryValueRepository { get; }

        public ChatObjectAppService(
            IChatObjectRepository repository,
            IChatObjectCategoryManager chatObjectCategoryManager,
            IChatObjectManager chatObjectManager,
            ISessionPermissionChecker sessionPermissionChecker,
            IRepository<EntryName, Guid> entryNameRepository,
            IRepository<EntryValue, Guid> entryValueRepository,
            IRepository<ChatObjectTargetEntryValue> chatObjectTargetEntryValueRepository) : base(repository)
        {
            ChatObjectCategoryManager = chatObjectCategoryManager;
            ChatObjectManager = chatObjectManager;
            SessionPermissionChecker = sessionPermissionChecker;
            EntryNameRepository = entryNameRepository;
            EntryValueRepository = entryValueRepository;
            ChatObjectTargetEntryValueRepository = chatObjectTargetEntryValueRepository;
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
            await Task.CompletedTask;
            return ObjectMapper.Map<ChatObject, ChatObjectDetailDto>(entity);
        }

        protected virtual async Task<ChatObjectDestinationDetailDto> MapToEntityDestinationDetailAsync(ChatObject entity)
        {
            await Task.CompletedTask;
            return ObjectMapper.Map<ChatObject, ChatObjectDestinationDetailDto>(entity);
        }
        [HttpGet]
        public async Task<ChatObjectDetailDto> GetDetailAsync(long id)
        {
            var entity = await ChatObjectManager.GetAsync(id);

            return await MapToEntityDetailAsync(entity);
        }

        protected virtual async Task CheckEntriesInputAsync(ChatObject chatObject, Dictionary<Guid, List<EntryValueInput>> input)
        {

            foreach (var item in input)
            {
                var entryName = Assert.NotNull(await EntryNameRepository.FindAsync(item.Key), $"不存在EntryNameId:{item.Key}");

                Assert.If(entryName.IsRequired && item.Value.Count == 0, $"${entryName.Name}必填");

                Assert.If(item.Value.Count > entryName.MaxCount, $"${entryName.Name}最大个数：{entryName.MaxCount}");

                Assert.If(item.Value.Count < entryName.MinCount, $"${entryName.Name}最小个数：{entryName.MinCount}");

                foreach (var entryValue in item.Value)
                {
                    Assert.If(entryValue.Value.Length > entryName.MaxLenth, $"${entryName.Name}[{item.Value.IndexOf(entryValue) + 1}]最大长度：{entryName.MaxLenth}");

                    Assert.If(entryValue.Value.Length < entryName.MinLenth, $"${entryName.Name}[{item.Value.IndexOf(entryValue) + 1}]最小长度：{entryName.MinLenth}");

                    //if (entryName.IsUniqued)
                    //{
                    //    var isAny = !await EntryValueRepository.AnyAsync(x => x.EntryNameId == entryName.Id && x.Value == entryValue.Value);
                    //    Assert.If(isAny, $"${entryName.Name}已经存在值：{entryValue.Value}");
                    //}
                }
            }
        }


        /// <summary>
        /// Create | Update | Delete items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="second"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        protected virtual async Task<List<T>> CudItemsAsync<T>(ICollection<T> source, ICollection<T> second, IEqualityComparer<T> comparer)
        {
            //delete
            var deleteItems = source.Except(second, comparer);

            foreach (var item in deleteItems)
            {
                source.Remove(item);
            }

            //create
            var createItems = second.Except(source, comparer);

            //modify
            var modifyItems = source.Intersect(second, comparer);

            foreach (var item in modifyItems)
            {
                TryToSetLastModificationTime(item);
            }

            await Task.CompletedTask;

            //reset
            return createItems.Concat(modifyItems).ToList();
        }

        protected virtual async Task<List<T>> FormatItemsAsync<T>(Dictionary<Guid, List<EntryValueInput>> input, Func<Guid, string, Task<T>> createOrFindEntity)
        {
            var inputItems = new List<T>();

            foreach (var entry in input)
            {
                Assert.If(!await EntryNameRepository.AnyAsync(x => x.Id == entry.Key), $"不存在EntryNameId:{entry.Key}");

                foreach (var valueInput in entry.Value)
                {
                    inputItems.Add(await createOrFindEntity(entry.Key, valueInput.Value));
                }
            }
            return inputItems;
        }

        [HttpPost]
        public async Task<ChatObjectDetailDto> SetEntriesAsync(long id, Dictionary<Guid, List<EntryValueInput>> input)
        {
            var entity = await ChatObjectManager.GetAsync(id);

            await CheckEntriesInputAsync(entity, input);

            //owner.Entries?.Clear();

            var inputItems = await FormatItemsAsync(input, async (Guid key, string value) =>
            {
                var entryValue = await EntryValueRepository.FirstOrDefaultAsync(x => x.EntryNameId == key && x.Value == value)
                                   ?? new EntryValue(GuidGenerator.Create(), key, value);
                return new ChatObjectEntryValue()
                {
                    OwnerId = entity.Id,
                    Owner = entity,
                    EntryValue = entryValue,
                    EntryValueId = entryValue.Id
                };
            });

            var entityComparer = new EntityComparer<ChatObjectEntryValue>(x => x.GetKeys().JoinAsString(","));

            entity.Entries = await CudItemsAsync(entity.Entries, inputItems, entityComparer);

            return await MapToEntityDetailAsync(entity);
        }

        [HttpPost]
        public async Task<ChatObjectDestinationDetailDto> SetDestinationEntriesAsync(long ownerId, long destinationId, Dictionary<Guid, List<EntryValueInput>> input)
        {
            var owner = await ChatObjectManager.GetAsync(ownerId);

            var destination = await ChatObjectManager.GetAsync(destinationId);

            await CheckEntriesInputAsync(owner, input);

            //owner.Entries?.Clear();

            var inputItems = await FormatItemsAsync(input, async (Guid key, string value) =>
            {
                var entryValue = await EntryValueRepository.FirstOrDefaultAsync(x => x.EntryNameId == key && x.Value == value)
                                   ?? new EntryValue(GuidGenerator.Create(), key, value);
                return new ChatObjectTargetEntryValue()
                {
                    OwnerId = owner.Id,
                    Owner = owner,
                    EntryValue = entryValue,
                    EntryValueId = entryValue.Id,
                    Destination = destination,
                    DestinationId = destination.Id,
                };
            });

            var entityComparer = new EntityComparer<ChatObjectTargetEntryValue>(x => x.GetKeys().JoinAsString(","));

            var targetEntries = (await ChatObjectTargetEntryValueRepository.GetQueryableAsync())
                .Where(x => x.OwnerId == owner.Id && x.DestinationId == destination.Id)
                //.Where(x=> input.Keys.Contains(x.EntryValue.EntryNameId))
                .ToList();

            //delete
            var deleteItems = targetEntries.Except(inputItems, entityComparer).ToList();

            await ChatObjectTargetEntryValueRepository.DeleteManyAsync(deleteItems, autoSave: true);

            foreach (var item in deleteItems)
            {
                targetEntries.Remove(item);
            }

            //create
            var createItems = inputItems.Except(targetEntries, entityComparer).ToList();

            await ChatObjectTargetEntryValueRepository.InsertManyAsync(createItems, autoSave: true);

            //modify
            var modifyItems = targetEntries.Intersect(inputItems, entityComparer).ToList();

            foreach (var item in modifyItems)
            {
                TryToSetLastModificationTime(item);
            }

            await ChatObjectTargetEntryValueRepository.UpdateManyAsync(modifyItems, autoSave: true);

            var entity = await ChatObjectManager.GetAsync(destinationId);

            entity.SetViewerEntries(targetEntries);

            return await MapToEntityDestinationDetailAsync(entity);
        }

    }
}
