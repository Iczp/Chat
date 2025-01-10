using IczpNet.AbpCommons;
using IczpNet.Chat.WalletBusinesses;
using IczpNet.Chat.Wallets;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.WalletOrders;

public class WalletOrderManager : DomainService, IWalletOrderManager
{
    protected IRepository<WalletBusiness, string> WalletBusinessRepository { get; }
    protected IWalletManager WalletManager { get; }
    public IWalletOrderNoGenerator WalletOrderNoGenerator { get; }
    protected IRepository<WalletOrder, Guid> Repository { get; }

    public WalletOrderManager(
        IWalletManager walletManager,
        IRepository<WalletBusiness, string> walletBusinessRepository,
        IWalletOrderNoGenerator walletOrderNoGenerator,
        IRepository<WalletOrder, Guid> repository)
    {
        WalletManager = walletManager;
        WalletBusinessRepository = walletBusinessRepository;
        WalletOrderNoGenerator = walletOrderNoGenerator;
        Repository = repository;
    }

    public async Task<WalletOrder> CreateAsync(long ownerId, string businessId, string title, string description, decimal amount)
    {

        Assert.If(amount <= 0, $"金额出错:{amount}");

        var entity = new WalletOrder(
            id: GuidGenerator.Create(),
            orderNo: await WalletOrderNoGenerator.MakeAsync(),
            wallet: await WalletManager.GetWalletAsync(ownerId),
            business: await WalletBusinessRepository.GetAsync(businessId),
            title: title,
            description: description,
            amount: amount,
            expireTime: Clock.Now.AddMinutes(30));

        return await Repository.InsertAsync(entity, autoSave: true);
    }

    public async Task<WalletOrder> UpdateAsync(Guid orderId, string title, string description, decimal amount)
    {
        Assert.If(amount <= 0, $"金额出错:{amount}");

        var entity = await Repository.GetAsync(orderId);

        Assert.If(entity.ExpireTime < Clock.Now || entity.IsExpired, $"过期的订单");

        Assert.If(entity.IsEnabled, $"禁用的订单");

        Assert.If(entity.Status != Enums.WalletOrderStatus.Pending, $"订单状态[{entity.Status}]不允许变更");

        entity.Amount = amount;

        entity.Title = title;

        entity.Description = description;

        return await Repository.UpdateAsync(entity, autoSave: true);
    }

    public virtual async Task<WalletOrder> CloseAsync(Guid orderId)
    {
        var entity = await Repository.GetAsync(orderId);

        Assert.If(entity.Status != Enums.WalletOrderStatus.Pending, $"订单状态[{entity.Status}]不允许变更");

        entity.Close();

        return await Repository.UpdateAsync(entity, autoSave: true);
    }

    public virtual Task<WalletOrder> SuccessAsync(Guid orderId)
    {
        throw new NotImplementedException();
    }
}
