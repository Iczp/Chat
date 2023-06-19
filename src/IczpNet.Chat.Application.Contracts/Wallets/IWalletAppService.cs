using IczpNet.Chat.Wallets.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Wallets;

public interface IWalletAppService :
    ICrudChatAppService<
        WalletDetailDto,
        WalletDto,
        Guid,
        WalletGetListInput>
{
    /// <summary>
    /// 充值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public Task<WalletDto> RechargeAsync(RechargeInput input);
}
