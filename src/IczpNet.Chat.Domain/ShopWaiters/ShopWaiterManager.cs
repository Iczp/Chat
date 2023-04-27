using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ShopWaiters
{
    public class ShopWaiterManager : DomainService, IShopWaiterManager
    {
        protected IChatObjectRepository Repository { get; }
        protected IChatObjectManager ChatObjectManager { get; }
        protected IChatObjectTypeManager ChatObjectTypeManager { get; }
        public ShopWaiterManager(IChatObjectRepository repository,
            IChatObjectManager chatObjectManager,
            IChatObjectTypeManager chatObjectTypeManager)
        {
            Repository = repository;
            ChatObjectManager = chatObjectManager;
            ChatObjectTypeManager = chatObjectTypeManager;
        }

        protected virtual async Task CheckCreateAsync(long shopKeeperId, string name)
        {
            Assert.If(await Repository.AnyAsync(x => x.ParentId == shopKeeperId && x.Name == name), $"already exists name:{name}");
        }

        protected virtual async Task CheckUpdateAsync(ChatObject entity, string name)
        {
            Assert.If(await Repository.AnyAsync(x => x.ParentId == entity.ParentId && x.Id != entity.Id && x.Name == name), $"already exists name:{name}");
        }

        public virtual async Task<ChatObject> CreateAsync(long shopKeeperId, string name)
        {
            await CheckCreateAsync(shopKeeperId, name);

            var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.ShopWaiter);

            var shopWaiter = await ChatObjectManager.CreateAsync(
                new ChatObject(
                    name: name,
                    chatObjectType: chatObjectType,
                    parentId: shopKeeperId),
                isUnique: false);

            return shopWaiter;
        }

        public virtual async Task<ChatObject> UpdateAsync(long id, string name)
        {
            var entity = await Repository.GetAsync(id);

            await CheckUpdateAsync(entity, name);

            return await ChatObjectManager.UpdateAsync(entity, isUnique: false);
        }
    }
}
