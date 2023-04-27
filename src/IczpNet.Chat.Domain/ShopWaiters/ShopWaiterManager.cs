using IczpNet.Chat.ChatObjects;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ShopWaiters
{
    public class ShopWaiterManager : DomainService, IShopWaiterManager
    {
        protected IChatObjectRepository Repository { get; }
        protected IChatObjectManager ChatObjectManager { get; }

        public ShopWaiterManager(IChatObjectRepository repository, IChatObjectManager chatObjectManager)
        {
            Repository = repository;
            ChatObjectManager = chatObjectManager;
        }

        
    }
}
