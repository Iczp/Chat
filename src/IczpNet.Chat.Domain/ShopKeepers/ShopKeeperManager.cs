using IczpNet.Chat.ChatObjects;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ShopKeepers;

public class ShopKeeperManager : DomainService, IShopKeeperManager
{
    protected IChatObjectRepository Repository { get; }
    protected IChatObjectManager ChatObjectManager { get; }

    public ShopKeeperManager(IChatObjectRepository repository, IChatObjectManager chatObjectManager)
    {
        Repository = repository;
        ChatObjectManager = chatObjectManager;
    }

    
}
