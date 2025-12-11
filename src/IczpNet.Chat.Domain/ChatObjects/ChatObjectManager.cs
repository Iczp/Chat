using Castle.Core.Internal;
using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace IczpNet.Chat.ChatObjects;

public class ChatObjectManager(IChatObjectRepository repository) : TreeManager<ChatObject, long, ChatObjectInfo>(repository), IChatObjectManager
{

    protected IChatObjectTypeManager ChatObjectTypeManager => LazyServiceProvider.LazyGetRequiredService<IChatObjectTypeManager>();
    protected IMessageSender MessageSender => LazyServiceProvider.LazyGetRequiredService<IMessageSender>();
    protected ISessionGenerator SessionGenerator => LazyServiceProvider.LazyGetRequiredService<ISessionGenerator>();
    protected IDistributedCache<List<long>, Guid> UserChatObjectCache => LazyServiceProvider.LazyGetRequiredService<IDistributedCache<List<long>, Guid>>();
    protected IDistributedCache<List<long>, ChatObjectSearchCacheKey> SearchCache => LazyServiceProvider.LazyGetRequiredService<IDistributedCache<List<long>, ChatObjectSearchCacheKey>>();
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

        return await SearchCache.GetOrAddAsync(new ChatObjectSearchCacheKey(keyword),
            async () => (await Repository.GetQueryableAsync())
             .WhereIf(!keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(keyword) || x.NameSpellingAbbreviation.Contains(keyword))
             .Select(x => x.Id)
             .ToList());
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

    public virtual async Task<List<long>> GetIdListByUserIdAsync(Guid userId)
    {
        //return (await Repository.GetQueryableAsync())
        //    .Where(x => x.AppUserId == userId)
        //    .Select(x => x.Id)
        //    .ToList();
        return await UserChatObjectCache.GetOrAddAsync(userId, async () =>
        {
            return (await Repository.GetQueryableAsync())
            .Where(x => x.AppUserId == userId)
            .Select(x => x.Id)
            .ToList();
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
}
