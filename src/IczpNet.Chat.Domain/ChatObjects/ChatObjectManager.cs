using Castle.Core.Internal;
using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ChatObjectCategoryUnits;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ChatObjects;

public class ChatObjectManager(IChatObjectRepository repository) : TreeManager<ChatObject, long, ChatObjectInfo>(repository), IChatObjectManager
{
    protected IChatObjectRepository ChatObjectRepository { get; } = repository;
    protected IChatObjectCategoryManager ChatObjectCategoryManager => LazyServiceProvider.LazyGetRequiredService<IChatObjectCategoryManager>();
    protected IChatObjectTypeManager ChatObjectTypeManager => LazyServiceProvider.LazyGetRequiredService<IChatObjectTypeManager>();
    protected IRepository<ChatObjectType, string> ChatObjectTypeRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<ChatObjectType, string>>();
    protected IMessageSender MessageSender => LazyServiceProvider.LazyGetRequiredService<IMessageSender>();
    protected ISessionGenerator SessionGenerator => LazyServiceProvider.LazyGetRequiredService<ISessionGenerator>();
    protected IDistributedCache<List<long>, Guid> UserChatObjectCache => LazyServiceProvider.LazyGetRequiredService<IDistributedCache<List<long>, Guid>>();
    protected IDistributedCache<ChatObjectSearchCacheItem, ChatObjectSearchCacheKey> SearchCache => LazyServiceProvider.LazyGetRequiredService<IDistributedCache<ChatObjectSearchCacheItem, ChatObjectSearchCacheKey>>();
    protected ISessionUnitRepository SessionUnitRepository => LazyServiceProvider.LazyGetRequiredService<ISessionUnitRepository>();

    public virtual async Task<IQueryable<long>> QueryByKeywordAsync(string keyword)
    {
        if (keyword.IsNullOrWhiteSpace())
        {
            return null;
        }

        return (await Repository.GetQueryableAsync())
            .WhereIf(!keyword.IsNullOrWhiteSpace(), x => x.Name.StartsWith(keyword) || x.NameSpellingAbbreviation.StartsWith(keyword))
            .Select(x => x.Id)
            ;
    }

    public virtual async Task<List<long>> SearchKeywordByCacheAsync(string keyword)
    {
        if (keyword.IsNullOrWhiteSpace())
        {
            return null;
        }

        var result = await SearchCache.GetOrAddAsync(new ChatObjectSearchCacheKey(keyword),
            async () =>
            {
                var idList = (await Repository.GetQueryableAsync())
                 .WhereIf(!keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(keyword) || x.NameSpellingAbbreviation.Contains(keyword))
                 .Select(x => x.Id)
                 .ToList();
                return new ChatObjectSearchCacheItem(idList);
            });

        return result.ChatObjectIdList;
    }


    protected override async Task CheckExistsByCreateAsync(ChatObject inputEntity)
    {
        Assert.If(await Repository.AnyAsync(x => x.Name == inputEntity.Name), $"Already exists Name:{inputEntity.Name}");

        Assert.If(!inputEntity.Code.IsNullOrEmpty() && await Repository.AnyAsync(x => x.Code == inputEntity.Code), $"Already exists CodeConnectionId:{inputEntity.Code}");
    }

    protected override async Task CheckExistsByUpdateAsync(ChatObject inputEntity)
    {
        Assert.If(!inputEntity.Code.IsNullOrEmpty() &&
            await Repository.AnyAsync(x => (x.Code == inputEntity.Code) && !x.Id.Equals(inputEntity.Id)),
            $"Already exists code[{inputEntity.Code}]");
    }

    public override Task<ChatObject> CreateAsync(ChatObject inputEntity, bool isUnique = true)
    {
        return base.CreateAsync(inputEntity, isUnique);
    }
    public virtual async Task<ChatObject> FindByCodeAsync(string code)
    {
        return await Repository.FindAsync(x => x.Code == code);
    }

    public virtual async Task<ChatObject> GetOrAddGroupAssistantAsync()
    {
        var entity = await FindByCodeAsync(ChatConsts.GroupAssistant);

        if (entity == null)
        {
            var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Robot);

            entity = new ChatObject("群助手", nameof(ChatConsts.GroupAssistant), chatObjectType, null)
            {
                Description = "我是机器人：加群",
            };
            entity.SetIsStatic(true);
            await CreateAsync(entity);

            Logger.LogDebug($"Cteate chatObject by code:{entity.Code}");
        }
        return entity;
    }

    public virtual async Task<ChatObject> GetOrAddPrivateAssistantAsync()
    {
        var entity = await FindByCodeAsync(ChatConsts.PrivateAssistant);

        if (entity == null)
        {
            var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Robot);
            //ChatConsts.PrivateAssistant.GetDescription()
            var description = ChatConsts.PrivateAssistant.GetType().GetAttribute<DescriptionAttribute>()?.Description;

            entity = new ChatObject(description ?? "私人助理", nameof(ChatConsts.PrivateAssistant), chatObjectType, null)
            {
                Description = "我是机器人,会发送私人消息、推送服务等",
            };
            entity.SetIsStatic(true);

            await CreateAsync(entity);

            Logger.LogDebug($"Cteate chatObject by code:{entity.Code}");
        }
        return entity;
    }

    public virtual async Task<ChatObject> GetOrAddNewsAsync()
    {
        var entity = await FindByCodeAsync(ChatConsts.News);

        if (entity == null)
        {
            var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Subscription);
            //ChatConsts.PrivateAssistant.GetDescription()
            var description = ChatConsts.PrivateAssistant.GetType().GetAttribute<DescriptionAttribute>()?.Description;

            entity = new ChatObject(description ?? "新闻", nameof(ChatConsts.News), chatObjectType, null)
            {
                Description = "推送新闻服务等",
            };
            entity.SetIsStatic(true);

            await CreateAsync(entity);

            Logger.LogDebug($"Cteate chatObject by code:{entity.Code}");
        }
        return entity;
    }
    public virtual async Task<ChatObject> GetOrAddNotifyAsync()
    {
        var entity = await FindByCodeAsync(ChatConsts.Notify);

        if (entity == null)
        {
            var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Official);
            //ChatConsts.PrivateAssistant.GetDescription()
            var description = ChatConsts.PrivateAssistant.GetType().GetAttribute<DescriptionAttribute>()?.Description;

            entity = new ChatObject(description ?? "通知", nameof(ChatConsts.Notify), chatObjectType, null)
            {
                Description = "推送通知服务等",
            };
            entity.SetIsStatic(true);

            await CreateAsync(entity);

            Logger.LogDebug($"Cteate chatObject by code:{entity.Code}");
        }
        return entity;
    }

    public override async Task<ChatObject> UpdateAsync(ChatObject entity, long? newParentId, bool isUnique = true)
    {
        Assert.If(entity.ObjectType == ChatObjectTypeEnums.ShopWaiter && !newParentId.HasValue, "[ShopWaiter] ParentId is null");

        if (entity.ObjectType == ChatObjectTypeEnums.Room)
        {


        }
        return await base.UpdateAsync(entity, newParentId, isUnique);
    }

    public virtual async Task<List<ChatObject>> GetListByUserIdAsync(Guid userId)
    {
        return await Repository.GetListAsync(x => x.AppUserId == userId);
    }

    [UnitOfWork(isTransactional: false)]
    public virtual async Task<List<long>> GetIdListByUserIdAsync(Guid userId)
    {
        //return (await Repository.GetQueryableAsync())
        //    .Where(x => x.AppUserId == userId)
        //    .Select(x => x.Id)
        //    .ToList();
        return await UserChatObjectCache.GetOrAddAsync(userId, async () =>
        {
            var query = (await Repository.GetQueryableAsync())
                .Where(x => x.AppUserId == userId)
                .Select(x => x.Id);
            return await AsyncExecuter.ToListAsync(query);
        });
    }

    public virtual async Task<List<long>> GetIdListByNameAsync(List<string> nameList)
    {
        var query = (await Repository.GetQueryableAsync())
            .Where(x => nameList.Contains(x.Name))
            .Select(x => x.Id)
            ;
        return await AsyncExecuter.ToListAsync(query);
    }

    public virtual async Task<List<ChatObject>> GetAllListAsync(ChatObjectTypeEnums objectType)
    {
        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.ObjectType == objectType)
            ;
        return await AsyncExecuter.ToListAsync(query);
    }

    public virtual async Task<ChatObject> CreatePersonalAsync(string name, string code, Action<ChatObject> action = null)
    {
        var entity = await FindByCodeAsync(code);

        if (entity == null)
        {
            var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Personal);

            var chatObject = new ChatObject(name, code, chatObjectType, null);

            action?.Invoke(chatObject);

            entity = await base.CreateAsync(chatObject, isUnique: true);
        }
        else
        {
            //action?.Invoke(entity);
        }
        return entity;
    }

    public virtual async Task<ChatObject> CreateShopKeeperAsync(string name, string code)
    {
        var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.ShopKeeper);

        var shopKeeper = await base.CreateAsync(new ChatObject(name, code, chatObjectType, null), isUnique: false);

        return shopKeeper;
    }

    public virtual async Task<ChatObject> CreateShopWaiterAsync(long shopKeeperId, string name, string code)
    {
        var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.ShopWaiter);

        var shopWaiter = await base.CreateAsync(new ChatObject(name, code, chatObjectType, shopKeeperId), isUnique: false);

        return shopWaiter;
    }

    public virtual async Task<ChatObject> CreateRobotAsync(string name, string code)
    {
        var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Robot);

        var robot = await base.CreateAsync(new ChatObject(name, code, chatObjectType, null), isUnique: false);

        return robot;
    }

    public virtual async Task<ChatObject> CreateSquareAsync(string name, string code)
    {
        var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Square);

        var square = await base.CreateAsync(new ChatObject(name, code, chatObjectType, null), isUnique: false);

        return square;
    }

    public virtual Task<ChatObject> CreateSubscriptionAsync(string name, string code)
    {
        throw new NotImplementedException();
    }

    public virtual Task<ChatObject> CreateOfficialAsync(string name, string code)
    {
        throw new NotImplementedException();
    }

    public virtual Task<ChatObject> CreateAnonymousAsync(string name, string code)
    {
        throw new NotImplementedException();
    }

    public virtual async Task<ChatObject> UpdateAsync(long id, Action<ChatObject> action, bool isUnique = true)
    {
        var entity = await Repository.GetAsync(id);

        action?.Invoke(entity);

        return await base.UpdateAsync(entity, entity.ParentId, isUnique: isUnique);
    }

    public virtual async Task<ChatObject> UpdateAsync(ChatObject entity, Action<ChatObject> action, bool isUnique = true)
    {
        action?.Invoke(entity);

        return await base.UpdateAsync(entity, entity.ParentId, isUnique: isUnique);
    }

    public virtual async Task<ChatObject> UpdateNameAsync(ChatObject entity, string name)
    {
        entity.SetName(name);

        //var count = await SessionUnitManager.BatchUpdateNameAsync(entity.Id, entity.Name, entity.NameSpelling, entity.NameSpellingAbbreviation);

        //Logger.LogInformation($"SessionUnitManager.BatchUpdateNameAsync:{count}");

        return await base.UpdateAsync(entity, entity.ParentId, isUnique: true);
    }

    public virtual async Task<ChatObject> UpdateNameAsync(long id, string name)
    {
        var entity = await Repository.GetAsync(id);

        return await UpdateNameAsync(entity, name);
    }

    public virtual async Task<bool> IsSomeRootAsync(params long[] idList)
    {
        Assert.If(idList.Length < 2, "idList.Length < 2");

        var queryable = (await Repository.GetQueryableAsync()).Where(x => idList.Contains(x.Id));

        var parent = queryable.FirstOrDefault(x => x.ParentId == null);

        if (parent != null)
        {
            var sp = parent.GetSplitString();
            //return queryable.Any(x => x.Id != parent.Id && x.ParentId == parent.Id);
            return queryable.Any(x => (x.FullPath + sp).StartsWith($"{parent.Id}{sp}"));
        }

        return queryable.GroupBy(x => x.ParentId).Any();
    }

    public virtual async Task<ChatObject> BingAppUserIdAsync(long id, Guid appUserId)
    {
        var entity = await Repository.GetAsync(id);

        entity.SetAppUserId(appUserId);

        var count = await SessionUnitRepository.BatchUpdateAppUserIdAsync(entity.Id, appUserId);

        Logger.LogInformation($"SessionUnitRepository.BatchUpdateAppUserIdAsync:{count}");

        // Clear Cache
        await UserChatObjectCache.RemoveAsync(appUserId);

        return await base.UpdateAsync(entity, entity.ParentId, isUnique: false);
    }

    public async Task<ChatObject> GenerateByUserAsync(UserEto userInfo)
    {
        var name = userInfo.Surname ?? userInfo.Name ?? userInfo.UserName;

        var userChatObject = await CreatePersonalAsync(name: name, code: userInfo.UserName, x =>
        {
            x.Description = $"";
            //绑定用户
            x.SetAppUserId(userInfo.Id);
        });

        return userChatObject;
    }

    public override async Task<List<ChatObjectInfo>> GetManyByCacheAsync(List<long> idList)
    {
        var kvs = await ItemCache.GetOrAddManyAsync(idList, async keys =>
          {
              var entities = await Repository.GetListAsync(x => keys.Contains(x.Id));

              var dict = entities.ToDictionary(
                  x => x.Id,
                  x => ObjectMapper.Map<ChatObject, ChatObjectInfo>(x));

              var result = new List<KeyValuePair<long, ChatObjectInfo>>();

              foreach (var id in keys)
              {
                  dict.TryGetValue(id, out var cacheItem);
                  result.Add(new KeyValuePair<long, ChatObjectInfo>(id, cacheItem));
              }

              return result;
          });
        return kvs.Select(x => x.Value).ToList();

    }

    public virtual async Task<ChatObjectAdminListResult> GetAdminListAsync(
        ChatObjectAdminQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        filter ??= new ChatObjectAdminQueryFilter();

        var normalizedFilter = new ChatObjectAdminQueryFilter
        {
            Keyword = filter.Keyword?.Trim(),
            Code = filter.Code?.Trim(),
            AppUserId = filter.AppUserId,
            ChatObjectTypeId = filter.ChatObjectTypeId?.Trim(),
            ObjectType = filter.ObjectType,
            ObjectTypes = filter.ObjectTypes,
            CategoryIdList = filter.CategoryIdList,
            IncludeChildCategories = filter.IncludeChildCategories,
            IsEnabled = filter.IsEnabled,
            IsDefault = filter.IsDefault,
            IsPublic = filter.IsPublic,
            IsStatic = filter.IsStatic,
            IsActive = filter.IsActive,
            IsDeveloper = filter.IsDeveloper,
            Sorting = filter.Sorting,
            SkipCount = Math.Max(0, filter.SkipCount),
            MaxResultCount = Math.Clamp(filter.MaxResultCount, 1, 200)
        };

        List<Guid> resolvedCategoryIds = null;
        if (normalizedFilter.CategoryIdList != null && normalizedFilter.CategoryIdList.Count > 0)
        {
            if (normalizedFilter.IncludeChildCategories)
            {
                var categoryQuery = await ChatObjectCategoryManager.QueryCurrentAndAllChildsAsync(normalizedFilter.CategoryIdList);
                resolvedCategoryIds = await AsyncExecuter.ToListAsync(categoryQuery.Select(x => x.Id), cancellationToken);
            }
            else
            {
                resolvedCategoryIds = normalizedFilter.CategoryIdList.Distinct().ToList();
            }
        }

        return await ChatObjectRepository.GetAdminPagedListAsync(normalizedFilter, resolvedCategoryIds, cancellationToken);
    }

    public virtual async Task<ChatObjectAdminDetailResult> GetAdminDetailAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await ChatObjectRepository.FindAdminDetailAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ChatObject), id);
        }

        var categoryUnits = entity.ChatObjectCategoryUnitList?
            .Where(x => x.Category != null)
            .ToList() ?? [];

        var entryValues = entity.Entries?
            .Where(x => x.EntryValue != null)
            .Select(x => new ChatObjectAdminEntryValueModel
            {
                EntryNameId = x.EntryValue.EntryNameId,
                EntryName = x.EntryValue.EntryName?.Name,
                EntryValueId = x.EntryValueId,
                Value = x.EntryValue.Value
            })
            .ToList() ?? [];

        var sameUserCount = entity.AppUserId.HasValue
            ? await ChatObjectRepository.GetCountByAppUserIdAsync(entity.AppUserId.Value, cancellationToken)
            : 0;

        return new ChatObjectAdminDetailResult
        {
            Entity = entity,
            CategoryIds = categoryUnits.Select(x => x.CategoryId).Distinct().ToList(),
            CategoryNames = categoryUnits.Select(x => x.Category.Name).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList(),
            EntryValues = entryValues,
            SameUserChatObjectCount = sameUserCount
        };
    }

    public virtual async Task<ChatObjectAdminDetailResult> UpdateAdminAsync(
        long id,
        string name,
        string code,
        Genders? gender,
        string description,
        string portrait,
        string thumbnail,
        bool? isPublic = null,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken: cancellationToken);

        entity.SetName(name);
        entity.Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        if (gender.HasValue)
        {
            entity.Gender = gender.Value;
        }
        entity.Description = description;
        if (portrait != null)
        {
            entity.SetPortrait(portrait);
        }
        if (thumbnail != null)
        {
            entity.SetThumbnail(thumbnail);
        }
        if (isPublic.HasValue)
        {
            entity.SetIsPublic(isPublic.Value);
        }

        await base.UpdateAsync(entity, entity.ParentId, isUnique: true);

        return await GetAdminDetailAsync(id, cancellationToken);
    }

    public virtual async Task<ChatObjectAdminDetailResult> SetVerificationMethodAdminAsync(
        long id,
        VerificationMethods verificationMethod,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken: cancellationToken);
        entity.SetVerificationMethod(verificationMethod);
        await base.UpdateAsync(entity, entity.ParentId, isUnique: false);
        return await GetAdminDetailAsync(id, cancellationToken);
    }

    public virtual async Task<ChatObjectAdminDetailResult> SetIsEnabledAdminAsync(
        long id,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken: cancellationToken);
        if (entity.IsStatic && !isEnabled)
        {
            throw new BusinessException("ChatAdmin:CannotDisableStaticChatObject", "系统固有聊天对象不允许停用。");
        }

        entity.SetIsEnabled(isEnabled);
        await base.UpdateAsync(entity, entity.ParentId, isUnique: false);
        return await GetAdminDetailAsync(id, cancellationToken);
    }

    public virtual async Task<ChatObjectAdminDetailResult> CreateAdminAsync(
        ChatObjectAdminCreateArgs args,
        CancellationToken cancellationToken = default)
    {
        if (args == null || string.IsNullOrWhiteSpace(args.Name))
        {
            throw new UserFriendlyException("请输入聊天对象名称。");
        }

        var normalizedName = args.Name.Trim();
        var normalizedCode = string.IsNullOrWhiteSpace(args.Code) ? null : args.Code.Trim();
        var resolvedTypeId = !string.IsNullOrWhiteSpace(args.ChatObjectTypeId)
            ? args.ChatObjectTypeId.Trim()
            : (args.ObjectType?.ToString() ?? nameof(ChatObjectTypeEnums.Personal));

        var chatObjectType = await ChatObjectTypeRepository.FindAsync(resolvedTypeId, cancellationToken: cancellationToken);
        if (chatObjectType == null)
        {
            throw new UserFriendlyException($"聊天对象类型不存在: {resolvedTypeId}");
        }

        var entity = new ChatObject(normalizedName, normalizedCode, chatObjectType, null);
        entity.SetName(normalizedName);
        entity.Description = args.Description;
        if (args.Gender.HasValue)
        {
            entity.Gender = args.Gender.Value;
        }
        entity.SetIsEnabled(args.IsEnabled);
        entity.SetIsPublic(args.IsPublic);
        entity.SetVerificationMethod(args.VerificationMethod);
        if (args.Portrait != null)
        {
            entity.SetPortrait(args.Portrait);
        }
        if (args.Thumbnail != null)
        {
            entity.SetThumbnail(args.Thumbnail);
        }
        if (args.CategoryIds != null && args.CategoryIds.Count > 0)
        {
            entity.ChatObjectCategoryUnitList = args.CategoryIds
                .Distinct()
                .Select(cid => new ChatObjectCategoryUnit { CategoryId = cid })
                .ToList();
        }

        await base.CreateAsync(entity, isUnique: true);

        return await GetAdminDetailAsync(entity.Id, cancellationToken);
    }

    public virtual async Task DeleteAdminAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.FindAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ChatObject), id);
        }

        if (entity.IsStatic)
        {
            throw new BusinessException("ChatAdmin:CannotDeleteStaticChatObject", "系统内置聊天对象不允许删除。");
        }

        await Repository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
    }
}
