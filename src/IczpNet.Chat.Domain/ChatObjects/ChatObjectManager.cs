using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectManager : TreeManager<ChatObject, long, ChatObjectInfo>, IChatObjectManager
    {
        
        protected IChatObjectTypeManager ChatObjectTypeManager => LazyServiceProvider.LazyGetRequiredService<IChatObjectTypeManager>();
        protected IMessageSender MessageSender => LazyServiceProvider.LazyGetRequiredService<IMessageSender>();
        protected ISessionGenerator SessionGenerator => LazyServiceProvider.LazyGetRequiredService<ISessionGenerator>();
        protected IDistributedCache<List<long>, Guid> UserChatObjectCache => LazyServiceProvider.LazyGetRequiredService<IDistributedCache<List<long>, Guid>>();

        public ChatObjectManager(IChatObjectRepository repository) : base(repository)
        {

        }

        public virtual async Task<List<ChatObject>> GetListByUserId(Guid userId)
        {
            return await Repository.GetListAsync(x => x.AppUserId == userId);
        }

        public virtual Task<List<long>> GetIdListByUserId(Guid userId)
        {
            return UserChatObjectCache.GetOrAddAsync(userId, async () =>
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

        public virtual async Task<ChatObject> CreateShopKeeperAsync(string name)
        {
            var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.ShopKeeper);

            var shopKeeper = await base.CreateAsync(new ChatObject(name, chatObjectType, null), isUnique: false);

            return shopKeeper;
        }

        public virtual async Task<ChatObject> CreateShopWaiterAsync(long shopKeeperId, string name)
        {
            var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.ShopWaiter);

            var shopWaiter = await base.CreateAsync(new ChatObject(name, chatObjectType, shopKeeperId), isUnique: false);

            return shopWaiter;
        }

        public virtual async Task<ChatObject> CreateRobotAsync(string name)
        {
            var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Robot);

            var entity = await base.CreateAsync(new ChatObject(name, chatObjectType, null), isUnique: false);

            return entity;
        }

        public virtual async Task<ChatObject> CreateSquareAsync(string name)
        {
            var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Square);

            var entity = await base.CreateAsync(new ChatObject(name, chatObjectType, null), isUnique: false);

            return entity;
        }

        public virtual Task<ChatObject> CreateSubscriptionAsync(string name)
        {
            throw new NotImplementedException();
        }

        public virtual Task<ChatObject> CreateOfficialAsync(string name)
        {
            throw new NotImplementedException();
        }

        public virtual Task<ChatObject> CreateAnonymousAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
