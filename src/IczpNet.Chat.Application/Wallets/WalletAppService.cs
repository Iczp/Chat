﻿using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Wallets.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Wallets;

/// <summary>
/// 钱包
/// </summary>
public class WalletAppService
    : CrudChatAppService<
        Wallet,
        WalletDetailDto,
        WalletDto,
        Guid,
        WalletGetListInput>,
    IWalletAppService
{

    protected IWalletManager WalletManager { get; }

    public WalletAppService(
        IRepository<Wallet, Guid> repository,
        IWalletManager walletManager) : base(repository)
    {
        WalletManager = walletManager;
    }

    protected override async Task<IQueryable<Wallet>> CreateFilteredQueryAsync(WalletGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
            ;
    }

    /// <summary>
    /// 充值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<WalletDto> RechargeAsync(RechargeInput input)
    {
        var wallet = await WalletManager.GetWalletAsync(input.OwnerId);

        wallet = await WalletManager.RechargeAsync(wallet, input.OwnerId, input.Amount, input.Description);

        return ObjectMapper.Map<Wallet, WalletDto>(wallet);
    }
}
